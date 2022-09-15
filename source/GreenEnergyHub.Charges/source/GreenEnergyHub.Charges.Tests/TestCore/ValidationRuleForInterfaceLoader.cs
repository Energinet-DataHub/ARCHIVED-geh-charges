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
using System.Runtime.Serialization;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.TestCore.Reflection;

namespace GreenEnergyHub.Charges.Tests.TestCore
{
    public static class ValidationRuleForInterfaceLoader
    {
        public static List<ValidationRuleIdentifier> GetValidationRuleIdentifierForTypes(
            Assembly assemblyContainingTypeWithInterface, Type typeOfInterface)
        {
            var types = GetTypesWithInterface(assemblyContainingTypeWithInterface, typeOfInterface);

            return types.Select(type => new
                {
                    type, obj = FormatterServices.GetUninitializedObject(type),
                })
                .Select(t => (ValidationRuleIdentifier)t.type.GetProperties()
                    .Single(x => x.Name == nameof(ValidationRuleIdentifier))
                    .GetValue(t.obj, null)!)
                .ToList();
        }

        private static IEnumerable<Type> GetTypesWithInterface(Assembly assembly, Type type)
        {
            return assembly.GetLoadableTypes()
                .Where(p => type.IsAssignableFrom(p) && !p.IsInterface)
                .ToList()!;
        }
    }
}
