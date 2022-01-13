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
using System.Reflection;

namespace GreenEnergyHub.Charges.Tests.FunctionHost
{
    public delegate bool IsTypeRegistered(Type type);

    public class ConstructorValidator
    {
        private readonly IsTypeRegistered _canResolve;

        public ConstructorValidator(IsTypeRegistered canResolve)
        {
            _canResolve = canResolve;
        }

        internal ConstructorValidator(bool canResolve = false)
            : this(t => canResolve) { }

        /// <summary>
        /// Verify that all constructor conditions are meet
        /// </summary>
        /// <param name="type"><see cref="Type"/> to inspect</param>
        /// <returns>true if all dependencies can be fulfilled</returns>
        public bool Verify(Type type)
        {
            var constructor = GetConstructor(type);

            var types = GetConstructorTypes(constructor);
            foreach (var parameter in types)
            {
                if (!IsTypeRegistered(parameter))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Get <see cref="ConstructorInfo"/> for a type
        /// </summary>
        /// <param name="type">Find constructor in this type</param>
        /// <returns><see cref="ConstructorInfo"/> for the <paramref name="type"/></returns>
        /// Throws
        /// <exception cref="ArgumentNullException"> if <paramref name="type"/> is null</exception>
        /// <exception cref="InvalidOperationException"> if <paramref name="type" /> contains multiple constructors</exception>
        internal ConstructorInfo GetConstructor(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            var constructors = type.GetConstructors();

            if (constructors.Length > 1) throw new InvalidOperationException("Only one constructor is supported per type");
            return constructors[0];
        }

        /// <summary>
        /// Get constructor parameters
        /// </summary>
        /// <param name="constructorInfo">Constructor to inspect</param>
        /// <returns><see cref="IEnumerable{Type}"/> of parameter types</returns>
        /// Throws <exception cref="ArgumentNullException">if constructor info is null</exception>
        internal IEnumerable<Type> GetConstructorTypes(ConstructorInfo constructorInfo) =>
            constructorInfo?.GetParameters().Select(p => p.ParameterType) ?? throw new ArgumentNullException(nameof(constructorInfo));

        private bool IsTypeRegistered(Type type) => _canResolve.Invoke(type);
    }
}
