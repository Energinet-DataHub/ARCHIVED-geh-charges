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
using AutoFixture;
using GreenEnergyHub.Messaging.Protobuf.Tests.Tooling.AutoFixture.Customizations;

namespace GreenEnergyHub.Messaging.Protobuf.Tests.Tooling.AutoFixture.Extensions
{
    /// <summary>
    /// Fluent API for configuring object builder to use a given value for a constructor parameter.
    /// Inspired by: https://stackoverflow.com/questions/16819470/autofixture-automoq-supply-a-known-value-for-one-constructor-parameter/16954699#16954699
    /// </summary>
    public class SetParameterCreateProvider<TTypeToConstruct>
    {
        public SetParameterCreateProvider(IFixture fixture)
        {
            Fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        }

        private IFixture Fixture { get; }

        public SetParameterProvider<TTypeToConstruct> SetParameter(string parameterName)
        {
            return new SetParameterProvider<TTypeToConstruct>(this, parameterName);
        }

        public TTypeToConstruct Create()
        {
            var instance = Fixture.Create<TTypeToConstruct>();
            return instance;
        }

        internal void AddConstructorParameter<TTypeOfParam>(ConstructorParameterRelay<TTypeToConstruct, TTypeOfParam> constructorParameter)
        {
            var existingSpecimenBuilder = Fixture.Customizations.SingleOrDefault(customization =>
                customization.GetType() == constructorParameter.GetType() &&
                ((ConstructorParameterRelay<TTypeToConstruct, TTypeOfParam>)customization).ParameterName == constructorParameter.ParameterName);

            if (existingSpecimenBuilder != null)
            {
                Fixture.Customizations.Remove(existingSpecimenBuilder);
            }

            Fixture.Customizations.Add(constructorParameter);
        }
    }
}
