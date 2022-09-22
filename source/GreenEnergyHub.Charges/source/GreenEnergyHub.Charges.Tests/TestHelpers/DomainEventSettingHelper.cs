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
using GreenEnergyHub.Charges.Infrastructure.Core.Function;

namespace GreenEnergyHub.Charges.Tests.TestHelpers
{
    public static class DomainEventSettingHelper
    {
        public static Dictionary<string, string> GetDomainEventSettings(Type type)
        {
            var domainEventSettings = new Dictionary<string, string>();
            var fields = type.GetFields();
            foreach (var field in fields.Where(x => x.GetCustomAttributes().Any(a => a is DomainEventSettingAttribute)))
            {
                var name = field.Name;
                var value = field.GetValue(null)!.ToString()!;

                domainEventSettings.Add(name, value);
            }

            return domainEventSettings;
        }
    }
}
