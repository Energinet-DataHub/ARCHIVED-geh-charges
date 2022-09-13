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

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoFixture;
using Google.Protobuf;
using Google.Protobuf.Collections;

namespace GreenEnergyHub.Charges.TestCore.Attributes.Customizations
{
    public static class ProtobufMessageExtensions
    {
        /// <summary>
        /// Uses the AddManyTo fixture extension on all properties of a IMessage that implement the RepeatedField generic
        /// </summary>
        /// <param name="message">IMessage that we will find all RepeatedFields for</param>
        /// <param name="fixture">Fixture to use to populate the RepeatedFields with AddManyTo</param>
        public static void AddManyToRepeatedFields(this IMessage message, IFixture fixture)
        {
            var properties = message.GetType().GetProperties();

            foreach (var property in properties)
            {
                var t = property.PropertyType;
                if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(RepeatedField<>))
                {
                    AddManyToRepeatedField(message, property, fixture);
                }
            }
        }

        private static void AddManyToRepeatedField(IMessage message, PropertyInfo property, IFixture fixture)
        {
            var obj = property.GetValue(message);

            // Since the RepeatedField generic is using a runtime type we cannot use AddManyTo<T> as you normally would.
            // Instead, we use reflection to find and call the AddManyTo method with the correct runtime type
            typeof(CollectionFiller)
                .GetMethods()
                .Where(
                    m =>
                    {
                        var parameters = m.GetParameters();
                        if (m.Name != "AddManyTo" ||
                            parameters.Length != 2 ||
                            parameters[0].ParameterType != typeof(IFixture) ||
                            !parameters[1].ParameterType.IsGenericType ||
                            parameters[1].ParameterType.GetGenericTypeDefinition() != typeof(ICollection<>))
                        {
                            return false;
                        }

                        return true;
                    })
                .Single()
                .MakeGenericMethod(property.PropertyType.GenericTypeArguments[0])
                .Invoke(fixture, new[] { fixture, obj! });
        }
    }
}
