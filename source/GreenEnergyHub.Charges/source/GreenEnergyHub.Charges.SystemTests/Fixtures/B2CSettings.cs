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

namespace GreenEnergyHub.Charges.SystemTests.Fixtures
{
    /// <summary>
    /// Settings necessary for aquiring an access token from the B2C tenant using the client credentials flow.
    /// </summary>
    /// <param name="B2cTenantId"></param>
    /// <param name="BackendAppId"></param>
    /// <param name="TeamClientId"></param>
    /// <param name="TeamClientSecret"></param>
    public record B2CSettings(
        string B2cTenantId,
        string BackendAppId,
        string TeamClientId,
        string TeamClientSecret);
}
