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
    /// This enum is used to indicate the resolution of a charge price list.
    /// PT15M = 15 minutes | PT1H = hour | P1D = day | P1M = month.
    /// </summary>
    public enum Resolution
    {
        Unknown = 0,
        PT15M = 1,
        PT1H = 2,
        P1D = 3,
        P1M = 4,
    }
}
