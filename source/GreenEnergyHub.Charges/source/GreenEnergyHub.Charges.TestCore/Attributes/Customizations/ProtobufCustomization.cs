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
using System.Linq;
using System.Reflection;
using AutoFixture;
using AutoFixture.Dsl;
using AutoFixture.Kernel;
using Google.Protobuf;
using Xunit.Sdk;

namespace GreenEnergyHub.Charges.TestCore.Attributes.Customizations
{
    /// <summary>
    /// AutoFixture customization that uses reflect to find all implementations of protobuf messages in assemblies
    /// belonging to GreenEnergyHub.
    ///
    /// For each of these implementations it registers a customization that makes sure we populate all of the protobuf
    /// repeated fields with content
    /// </summary>
    public class ProtobufCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            FindAndCustomizeIMessageImplementations(fixture);
        }

        private static void FindAndCustomizeIMessageImplementations(IFixture fixture)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                if (assembly.FullName != null && assembly.FullName.StartsWith("GreenEnergyHub", StringComparison.InvariantCulture))
                {
                    FindAndCustomizeIMessageImplementations(fixture, assembly);
                }
            }
        }

        private static void FindAndCustomizeIMessageImplementations(IFixture fixture, Assembly assembly)
        {
            var types = assembly.GetTypes();

            foreach (var type in types)
            {
                if (!type.IsInterface
                    && type.GetInterfaces().Any(x =>
                        x.IsGenericType &&
                        x.GetGenericTypeDefinition() == typeof(IMessage<>)))
                {
                    CustomizeIMessageImplementation(fixture, type);
                }
            }
        }

        private static void CustomizeIMessageImplementation(IFixture fixture, Type messageType)
        {
            // Since the protobuf IMessage implementations are not known until at runtime, we cannot
            // use Customize<T> as we normally would. Instead, we use reflection and func delegates to setup
            // everything at runtime with the help of the ProtobufCustomizationFunction utility class
            var function = CreateProtobufCustomizationFunction(fixture, messageType);
            var functionMethod = function
                .GetType()
                .GetMethod("GetCustomization");

            typeof(IFixture)
                .GetMethods()
                .Where(
                    m =>
                    {
                        var parameters = m.GetParameters();
                        if (m.Name == "Customize" &&
                            parameters.Length == 1 &&
                            parameters[0].ParameterType.IsGenericType &&
                            parameters[0].ParameterType.GenericTypeArguments.Length == 2 &&
                            parameters[0].ParameterType.GenericTypeArguments[0].IsGenericType &&
                            parameters[0].ParameterType.GenericTypeArguments[0].GetGenericTypeDefinition() ==
                            typeof(ICustomizationComposer<>) &&
                            parameters[0].ParameterType.GenericTypeArguments[1] == typeof(ISpecimenBuilder))
                        {
                            return true;
                        }

                        return false;
                    })
                .Single()
                .MakeGenericMethod(messageType)
                .Invoke(fixture, new object[] { functionMethod?.Invoke(function, Array.Empty<object>()) ?? new NullException("Null encountered while invoking method to retrieve customization function") });
        }

        private static object CreateProtobufCustomizationFunction(IFixture fixture, Type messageType)
        {
            var functionType = typeof(ProtobufCustomizationFunction<>)
                .MakeGenericType(messageType);
            return Activator.CreateInstance(functionType, new object[] { fixture })!;
        }
    }
}
