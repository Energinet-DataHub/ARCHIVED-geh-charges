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
using GreenEnergyHub.Charges.QueryApi;
using GreenEnergyHub.Charges.WebApi.ModelPredicates;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GreenEnergyHub.Charges.WebApi.Controllers.V1;

[ApiController]
[ApiVersion(Version1)]
[Route("v{version:apiVersion}/[controller]")]
public class ChargesController : ControllerBase
{
    public const string Version1 = "1.0";
    private readonly IData _data;

    public ChargesController(IData data)
    {
        _data = data;
    }

    /// <summary>
    ///     Returns all charges
    /// </summary>
    /// <returns>Charges data or "404 Not Found"</returns>
    [HttpGet("GetAsync")]
    [MapToApiVersion(Version1)]
    public async Task<IActionResult> GetAsync()
    {
        var charges = await _data.Charges
            .AsChargeV1Dto()
            .ToListAsync()
            .ConfigureAwait(false);

        return Ok(charges);
    }
}
