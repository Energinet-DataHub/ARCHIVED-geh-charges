﻿// Copyright 2020 Energinet DataHub A/S
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

namespace GreenEnergyHub.Charges.WebApi.Controllers.V1
{
    [ApiController]
    [ApiVersion(Version1, Deprecated = true)]
    [Route("v{version:apiVersion}/[controller]")]
    [Route("[controller]")] // for backward compatibility
    public class ChargeLinksController : ControllerBase
    {
        public const string Version1 = "1.0";
        private readonly IData _data;

        public ChargeLinksController(IData data)
        {
            _data = data;
        }

        /// <summary>
        /// Returns all charge links data for a given metering point.
        ///
        /// This V1 can be removed when the BFF uses the new V2.
        /// </summary>
        /// <param name="meteringPointId">The 18-digits metering point identifier used by the Danish version of Green Energy Hub.
        /// Use 404 to get a "404 Not Found" response.</param>
        /// <returns>Charge links data or "404 Not Found"</returns>
        [HttpGet("GetAsync")]
        [MapToApiVersion(Version1)]
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
                .OrderBy(c => c.ChargeInformation.Type)
                .ThenBy(c => c.ChargeInformation.SenderProvidedChargeId)
                .ThenByDescending(c => c.StartDateTime)
                .AsChargeLinkV1Dto()
                .ToListAsync()
                .ConfigureAwait(false);

            return Ok(chargeLinks);
        }
    }
}
