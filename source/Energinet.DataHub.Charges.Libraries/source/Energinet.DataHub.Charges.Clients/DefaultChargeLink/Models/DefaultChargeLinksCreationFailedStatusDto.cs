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

using GreenEnergyHub.Charges.Contracts;

namespace Energinet.DataHub.Charges.Clients.DefaultChargeLink.Models
{
    /// <summary>
    /// The data needed by the Metering Point domain as a reply
    /// to a failed CreateDefaultChargeLinks request
    /// </summary>
    /// <param name="MeteringPointId">A unique id to specify the metering point.</param>
    /// <param name="ErrorCode">Tells why the CreateDefaultChargeLinks request has failed.</param>
    public sealed record DefaultChargeLinksCreationFailedStatusDto(string MeteringPointId, ErrorCode ErrorCode);
}
