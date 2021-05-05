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

namespace GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction
{
    /// <summary>
    /// This enum indicates the action requested by the sender, e.g. using Addition when sender wants to create a new charge price list.
    /// </summary>
    public enum ChargeEventFunction
    {
        Unknown = 0,
        Addition = 2,
        Deletion = 3,
        Change = 4,
    }
}
