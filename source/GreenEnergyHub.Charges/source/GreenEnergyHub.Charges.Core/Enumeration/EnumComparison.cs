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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace GreenEnergyHub.Charges.Core.Enumeration
{
    public static class EnumComparison
    {
        public static bool IsSubsetOf(
            [NotNull] Type enumType,
            Type comparisonType,
            EnumComparisonStrategy strategy)
        {
            var values = Enum.GetValues(enumType);

            var valueComparisonStrategy = EnumValueNameComparisonStrategyFactory.Create(strategy);

            foreach (Enum value in values)
            {
                var comparisonValue = ConvertToComparisonEnum(value!, comparisonType);

                if (!IsSameIntValue(value, comparisonValue))
                {
                    return false;
                }

                if (!valueComparisonStrategy.IsEquivalent(value, comparisonValue))
                {
                    return false;
                }
            }

            return true;
        }

        private static Enum ConvertToComparisonEnum(Enum value, Type comparisonType)
        {
            return (Enum)Enum.ToObject(comparisonType, Convert.ToInt32(value, CultureInfo.InvariantCulture));
        }

        private static bool IsSameIntValue(Enum valueOne, Enum valueTwo)
        {
            var oneAsInt = Convert.ToInt32(valueOne, CultureInfo.InvariantCulture);
            var twoAsInt = Convert.ToInt32(valueTwo, CultureInfo.InvariantCulture);

            return oneAsInt == twoAsInt;
        }
    }
}
