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

namespace GreenEnergyHub.Charges.Core.Enumeration
{
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
            var pos = s.IndexOf('_');
            if (pos < 0)
            {
                throw new ArgumentException(
                    "The provided name of a protobuf enum value does not comform to protobuf standard");
            }

            return s.Substring(pos + 1, s.Length - pos - 1);
        }
    }
}
