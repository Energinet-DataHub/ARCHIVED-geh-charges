// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using GreenEnergyHub.Messaging.Dispatching;
using GreenEnergyHub.Messaging.MessageRouting;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GreenEnergyHub.Messaging
{
    /// <summary>
    /// Static class which allows the Azure Functions runtime IServiceCollection
    /// to discover and register classes needed to run the Green Energy Hub
    /// project.
    /// </summary>
    public static class HandlerExtensions
    {
        /// <summary>
        /// Searches the provided assemblies, as well as the common assemblies,
        /// to find
        /// - HubRequestHandlers,
        /// - HubCommandHandlers,
        /// - IHubMessages,
        /// - IRuleEngines,
        /// - Rules (represented as a list of Types),
        /// - IEndpointResolver,
        /// - IHubMessageTypeMap,
        /// and automatically register and provide the instances on function startup.
        ///
        /// Note, in case of multiple implementations of IHubMessages with the same name - only one will be registered.
        /// See <see cref="FindImplementations"/> for more information
        /// </summary>
        /// <param name="services">The IServiceCollection services provided by the Azure Functions runtime to register our interfaces with.</param>
        /// <param name="customerAssemblies">A list of assemblies to search for interfaces to automatically register.</param>
        public static void AddGreenEnergyHub(
            this IServiceCollection services,
            params System.Reflection.Assembly[] customerAssemblies)
        {
            // Register framework-provided classes last, this is done to allow customerAssemblies to take priority over framework when finding implementations of IHubMessage
            var assemblies = customerAssemblies.Append(typeof(IHubMessage).Assembly).ToArray(); // creates a new sequence, not altering input.

            services.AddMediatR(assemblies);

            var implementationFound = FindImplementations(typeof(IHubMessage), assemblies);
            foreach (var messageType in implementationFound)
            {
                // Register with mapping class for the message type
                services.AddTransient(_ => new MessageRegistration(messageType));
            }

            services.AddSingleton<IHubRequestMediator, HubRequestMediator>();
            services.AddSingleton<IHubCommandMediator, HubCommandMediator>();
            services.AddSingleton<IHubMessageTypeMap, HubMessageTypeMap>();

            services.DiscoverValidation(assemblies);
        }

        /// <summary>
        /// Searches the provided <paramref name="assemblies"/> for implementations of <paramref name="interfaceToCheckFor"/>.
        ///
        /// Note, in case multiple types are found with the same Name, only one will be returned.
        /// Which Type is selected, is based on the owning assembly's position in <paramref name="assemblies"/>.
        /// Types from assemblies with lower indexes are prioritized.
        /// </summary>
        /// <param name="interfaceToCheckFor">A type to use, as a basis, for finding the types to register</param>
        /// <param name="assemblies">A sequence of assemblies, in which to search for implementations of <paramref name="interfaceToCheckFor"/></param>
        private static List<Type> FindImplementations(Type interfaceToCheckFor, System.Reflection.Assembly[] assemblies)
        {
            var hubMessageNameToTypeDictionary = new Dictionary<string, Type>();

            // Check the assemblies in reverse order to satisfy the post condition that the first encounter of a given name wins.
            // this works since '[type.Name] = type' adds/updates the type value stored, overriding values from less significant assemblies.
            for (var i = assemblies.Length - 1; i >= 0; i--)
            {
                var messageTypes = assemblies[i].GetTypes()
                    .Where(type => !type.IsInterface && type.GetInterfaces().Contains(interfaceToCheckFor));

                foreach (var type in messageTypes)
                {
                    hubMessageNameToTypeDictionary[type.Name] = type;
                }
            }

            return hubMessageNameToTypeDictionary.Values.ToList();
        }
    }
}
