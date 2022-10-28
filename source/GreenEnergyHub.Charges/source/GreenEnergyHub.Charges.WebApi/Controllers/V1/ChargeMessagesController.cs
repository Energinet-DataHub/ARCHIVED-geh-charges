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
using Energinet.DataHub.Charges.Contracts.ChargeMessage;
using GreenEnergyHub.Charges.QueryApi.QueryServices;
using Microsoft.AspNetCore.Mvc;

namespace GreenEnergyHub.Charges.WebApi.Controllers.V1;

[ApiController]
[ApiVersion(Version1)]
[Route("v{version:apiVersion}/[controller]")]
public class ChargeMessagesController : Controller
{
    public const string Version1 = "1.0";
    private readonly IChargeMessageQueryService _chargeMessageQueryService;

    public ChargeMessagesController(IChargeMessageQueryService chargeMessageQueryService)
    {
        _chargeMessageQueryService = chargeMessageQueryService;
    }

    /// <summary>
    /// Returns all charge messages based on search criteria
    /// </summary>
    /// <returns>Charge Messages</returns>
    [HttpPost("SearchAsync")]
    [MapToApiVersion(Version1)]
    public async Task<IActionResult> SearchAsync([FromBody] ChargeMessagesSearchCriteriaV1Dto searchCriteria)
    {
        var chargeMessages = await _chargeMessageQueryService.SearchAsync(searchCriteria).ConfigureAwait(false);

        return Ok(chargeMessages);
    }
}
