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
using System.Reflection;
using AutoFixture.Kernel;

namespace GreenEnergyHub.Messaging.Protobuf.Tests.Tooling.AutoFixture.Customizations
{
    /// <summary>
    /// If creating a type <typeparamref name="TTypeToConstruct"/> using a constructor,
    /// then return <see cref="ParameterValue"/>,
    /// for any parameter of type <typeparamref name="TValueType"/> with name <see cref="ParameterName"/>.
    ///
    /// Inspired by: https://stackoverflow.com/questions/16819470/autofixture-automoq-supply-a-known-value-for-one-constructor-parameter/16954699#16954699
    /// </summary>
    public class ConstructorParameterRelay<TTypeToConstruct, TValueType> : ISpecimenBuilder
    {
        public ConstructorParameterRelay(string parameterName, TValueType parameterValue)
        {
            ParameterName = parameterName ?? throw new ArgumentNullException(nameof(parameterName));
            ParameterValue = parameterValue;
        }

        public string ParameterName { get; }

        public TValueType ParameterValue { get; }

        public object Create(object request, ISpecimenContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (!(request is ParameterInfo parameter))
            {
                return new NoSpecimen();
            }

            if (parameter.Member.DeclaringType != typeof(TTypeToConstruct) ||
                parameter.Member.MemberType != MemberTypes.Constructor ||
                parameter.ParameterType != typeof(TValueType) ||
                parameter.Name != ParameterName)
            {
                return new NoSpecimen();
            }

            return ParameterValue!;
        }
    }
}
