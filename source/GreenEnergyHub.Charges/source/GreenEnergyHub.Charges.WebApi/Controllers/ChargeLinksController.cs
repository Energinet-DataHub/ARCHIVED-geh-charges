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

using System.Linq;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.QueryApi;
using GreenEnergyHub.Charges.QueryApi.QueryPredicates;
using GreenEnergyHub.Charges.WebApi.ModelPredicates;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GreenEnergyHub.Charges.WebApi.Controllers
{
    /// <summary>
    /// Version 1: Introduce action to get charge links.
    /// Version 2: Breaking changes to get charge links.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    [ApiVersion("1.0", Deprecated = true)]
    [ApiVersion("2.0")]
    public class ChargeLinksController : ControllerBase
    {
        private readonly IData _data;

        public ChargeLinksController(IData data)
        {
            _data = data;
        }

        /// <summary>
        /// Returns all charge links data for a given metering point. Currently it returns mocked data.
        ///
        /// This V1 can be removed when the BFF uses the new V2.
        /// </summary>
        /// <param name="meteringPointId">The 18-digits metering point identifier used by the Danish version of Green Energy Hub.
        /// Use 404 to get a "404 Not Found" response.</param>
        /// <returns>Mocked charge links data or "404 Not Found"</returns>
        [HttpGet("GetAsync")]
        [MapToApiVersion("1.0")]
        public async Task<IActionResult> GetV1Async(string meteringPointId)
        {
            if (meteringPointId == null)
                return BadRequest();

            var meteringPointExists = await _data
                .MeteringPoints
                .AnyAsync(m => m.MeteringPointId == meteringPointId)
                .ConfigureAwait(false);

            if (!meteringPointExists)
                return NotFound();

            var chargeLinks = await _data
                .ChargeLinks
                .ForMeteringPoint(meteringPointId)
                .OrderBy(c => c.Charge.Type)
                .ThenBy(c => c.Charge.SenderProvidedChargeId)
                .ThenByDescending(c => c.StartDateTime)
                .AsChargeLinkV1Dto()
                .ToListAsync()
                .ConfigureAwait(false);

            return Ok(chargeLinks);
        }

        /// <summary>
        /// Returns all charge links data for a given metering point. Currently it returns mocked data.
        /// </summary>
        /// <param name="meteringPointId">The 18-digits metering point identifier used by the Danish version of Green Energy Hub.
        /// Use 404 to get a "404 Not Found" response.</param>
        /// <returns>Mocked charge links data or "404 Not Found"</returns>
        [HttpGet("GetAsync")]
        [MapToApiVersion("2.0")]
        public async Task<IActionResult> GetAsync(string meteringPointId)
        {
            if (meteringPointId == null)
                return BadRequest();

            var meteringPointExists = await _data
                .MeteringPoints
                .AnyAsync(m => m.MeteringPointId == meteringPointId)
                .ConfigureAwait(false);

            if (!meteringPointExists)
                return NotFound();

            var chargeLinks = await _data
                .ChargeLinks
                .ForMeteringPoint(meteringPointId)
                .OrderBy(c => c.Charge.Type)
                .ThenBy(c => c.Charge.SenderProvidedChargeId)
                .ThenByDescending(c => c.StartDateTime)
                .AsChargeLinkV2Dto()
                .ToListAsync()
                .ConfigureAwait(false);

            return Ok(chargeLinks);
        }
    }
}
