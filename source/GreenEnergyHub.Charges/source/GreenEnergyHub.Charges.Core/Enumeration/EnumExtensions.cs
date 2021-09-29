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
    public static class EnumExtensions
    {
        public static TOutputEnum Cast<TOutputEnum>([NotNull]this Enum inputEnum)
          where TOutputEnum : Enum
        {
            if (inputEnum == null) throw new ArgumentNullException(nameof(inputEnum));

            var isDefined = Enum.IsDefined(typeof(TOutputEnum), (int)(object)inputEnum);
            if (!isDefined)
            {
                var message =
                    $"Cannot cast enum '{inputEnum.GetType().Name}.{inputEnum}={(int)(object)inputEnum}' " +
                    $"to enum type '{typeof(TOutputEnum).Name}' because it has no corresponding value.";
                throw new InvalidCastException(message);
            }

            return (TOutputEnum)inputEnum;
        }
    }
}
