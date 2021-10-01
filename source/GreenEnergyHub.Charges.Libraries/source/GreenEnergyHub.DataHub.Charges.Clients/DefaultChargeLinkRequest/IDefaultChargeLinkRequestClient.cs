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

using System.Threading.Tasks;

namespace GreenEnergyHub.DataHub.Charges.Libraries.DefaultChargeLinkRequest
{
    public interface IDefaultChargeLinkRequestClient
    {
        /// <summary>
        /// Request the Charges domain to create default charge links
        /// based on the supplied meteringPointIds entity's MeteringPointType.
        /// </summary>
        /// <param name="meteringPointId"></param>
        /// <param name="correlationId"></param>
        Task CreateDefaultChargeLinksRequestAsync(
            string meteringPointId,
            string correlationId);

        ValueTask DisposeAsync();
    }
}
