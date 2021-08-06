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
using AutoFixture.AutoMoq;
using AutoFixture.Dsl;
using AutoFixture.Kernel;
using Google.Protobuf;
using Xunit.Sdk;

namespace GreenEnergyHub.Charges.TestCore
{
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
                FindAndCustomizeIMessageImplementations(fixture, assembly);
            }
        }

        private static void FindAndCustomizeIMessageImplementations(IFixture fixture, Assembly assembly)
        {
            var types = assembly.GetTypes();

            foreach (var type in types)
            {
                if (type.GetInterfaces().Any(x =>
                    x.IsGenericType &&
                    x.GetGenericTypeDefinition() == typeof(IMessage<>)))
                {
                    CustomizeIMessageImplementation(fixture, type);
                }
            }
        }

        private static void CustomizeIMessageImplementation(IFixture fixture, Type t)
        {
            var customizationMethod = typeof(ProtobufCustomization)
                .GetMethod(
                    nameof(GetCustomization),
                    BindingFlags.NonPublic | BindingFlags.Static);

            var function = Delegate.CreateDelegate(
                customizationMethod!.ReturnType,
                customizationMethod!);

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
                .MakeGenericMethod(t)
                .Invoke(fixture, new object[] { function });

            System.Console.WriteLine("Seems ok - " + t);
        }

        private static Func<ICustomizationComposer<T>, ISpecimenBuilder> GetCustomization<T>(IFixture fixture)
        {
            return c => c.Do(
                e =>
                {
                    var message = e as IMessage;

                    if (message == null)
                    {
                        throw new NullException(
                            "GetCustomization was called on a type that was not an IMessage, resulting in a null error");
                    }

                    message.AddManyToRepeatedFields(fixture);
                });
        }
    }
}
