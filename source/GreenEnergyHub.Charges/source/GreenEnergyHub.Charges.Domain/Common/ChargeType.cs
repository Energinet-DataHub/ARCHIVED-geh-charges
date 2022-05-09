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

namespace GreenEnergyHub.Charges.Domain.Common
{
    /// <summary>
    /// The kind of charge: Subscription ("D01") | Fee ("D02") | Tariff ("D03").
    /// IMPORTANT: The enum is used in transportation so make sure not to change existing numbers.
    /// </summary>
    public enum ChargeType
    {
        Unknown = 0,
        Subscription = 1,
        Fee = 2,
        Tariff = 3,
    }
}
