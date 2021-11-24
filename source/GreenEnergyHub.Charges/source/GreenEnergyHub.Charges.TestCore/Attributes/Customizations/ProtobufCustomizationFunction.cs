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
using AutoFixture;
using AutoFixture.Dsl;
using AutoFixture.Kernel;
using Google.Protobuf;
using Xunit.Sdk;

namespace GreenEnergyHub.Charges.TestCore.Attributes.Customizations
{
    public class ProtobufCustomizationFunction<TProtobufMessage>
    {
        private readonly IFixture _fixture;

        public ProtobufCustomizationFunction(IFixture fixture)
        {
            _fixture = fixture;
        }

        public Func<ICustomizationComposer<TProtobufMessage>, ISpecimenBuilder> GetCustomization()
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

                    message.AddManyToRepeatedFields(_fixture);
                });
        }
    }
}
