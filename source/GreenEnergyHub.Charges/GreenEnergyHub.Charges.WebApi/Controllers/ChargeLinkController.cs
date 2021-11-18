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

using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.WebApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace GreenEnergyHub.Charges.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChargeLinkController : ControllerBase
    {
        public ChargeLinkController()
        {
            // TODO: Add log functionality
        }

        /// <summary>
        /// Returns all charge links data for a given metering point. Currently it returns mocked data.
        /// </summary>
        /// <param name="meteringPointId">The 18-digits metering point identifier used by the Danish version of Green Energy Hub.
        /// Use 404 to get a "404 Not Found" response.</param>
        /// <returns>Mocked charge links data or "404 Not Found"</returns>
        [HttpGet("GetChargeLinksByMeteringPointId")]
        public async Task<ActionResult> GetChargeLinksByMeteringPointIdAsync(string meteringPointId)
        {
            if (meteringPointId == "404")
            {
                return NotFound();
            }

            // Uses mocked charge links data - later this will be refactored to use actual data from storage.
            var mockDataText = await System.IO.File.ReadAllTextAsync(@"Files/ChargeLinksMockData.json").ConfigureAwait(false);

            var mockChargeLinksData = JsonSerializer.Deserialize<IEnumerable<ChargeLinkDto>>(mockDataText);

            return Ok(mockChargeLinksData);
        }
    }
}
