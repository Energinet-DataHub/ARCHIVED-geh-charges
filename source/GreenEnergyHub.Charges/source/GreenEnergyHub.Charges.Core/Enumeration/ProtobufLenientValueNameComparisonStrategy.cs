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
using System.Linq;
using System.Text.RegularExpressions;

namespace GreenEnergyHub.Charges.Core.Enumeration
{
    /// <summary>
    /// Protobuf lenient comparison allows the name of the enums being compared to be slightly lenient unlike
    /// the exact match strategy where the names has to exactly alike.
    ///
    /// In protobuf lenient comparison protobuf will have a prefix to the name, defined by one or more
    /// characters starting with an upper case character and ending at the character just before the next
    /// upper case character. For example PrefixEnumName would compare EnumName with the other enum, skipping the
    /// first Uppercase word. In addition, the remaining part of the enum value name will be allowed to match
    /// without regard to casing.
    /// </summary>
    public class ProtobufLenientValueNameComparisonStrategy : IEnumValueNameComparisonStrategy
    {
        public bool IsEquivalent([NotNull] Enum protobufValue, [NotNull] Enum comparisonValue)
        {
            var lenientString = GetLenientString(protobufValue);
            return string.Compare(
                lenientString,
                comparisonValue.ToString(),
                StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        private static string GetLenientString(Enum protobufValue)
        {
            var valueWithoutPrefix = GetStringWithoutPrefix(protobufValue.ToString());
            return valueWithoutPrefix.Replace("_", string.Empty);
        }

        private static string GetStringWithoutPrefix(string s)
        {
            var uppercaseWords = Regex.Matches(s, @"([A-Z][a-z0-9]*)")
                .Select(m => m.Value).ToList();

            if (uppercaseWords.Count < 2)
            {
                throw new ArgumentException(
                    "The provided name of a protobuf enum value does not adhere to protobuf standard naming: " + s);
            }

            uppercaseWords.Remove(uppercaseWords.First());
            return string.Join(null, uppercaseWords);
        }
    }
}
