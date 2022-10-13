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
using Energinet.Charges.Contracts.ChargePoint;
using GreenEnergyHub.Charges.QueryApi.QueryServices;
using Microsoft.AspNetCore.Mvc;

namespace GreenEnergyHub.Charges.WebApi.Controllers.V1;

[ApiController]
[ApiVersion(Version1)]
[Route("v{version:apiVersion}/[controller]")]
public class ChargePointsController : Controller
{
    public const string Version1 = "1.0";
    private readonly IChargePointQueryService _chargePointQueryService;

    public ChargePointsController(IChargePointQueryService chargePointQueryService)
    {
        _chargePointQueryService = chargePointQueryService;
    }

    /// <summary>
    ///     Returns all charge points based on search criteria
    /// </summary>
    /// <returns>Charge points or "404 Not Found"</returns>
    [HttpPost("SearchAsync")]
    [MapToApiVersion(Version1)]
    public async Task<IActionResult> SearchAsync([FromBody] ChargePointSearchCriteriaV1Dto chargePointSearchCriteria)
    {
        var chargePoints = await _chargePointQueryService
            .SearchAsync(chargePointSearchCriteria)
            .ConfigureAwait(false);

        return Ok(chargePoints);
    }
}
