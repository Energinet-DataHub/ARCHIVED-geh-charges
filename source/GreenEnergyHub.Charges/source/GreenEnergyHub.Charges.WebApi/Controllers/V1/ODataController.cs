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

using GreenEnergyHub.Charges.QueryApi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace GreenEnergyHub.Charges.WebApi.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/[controller]")]
    [Route("[controller]")] // for backward compatibility
    public class ODataController : Controller
    {
        private readonly IData _data;

        public ODataController(IData data)
        {
            _data = data;
        }

        [HttpGet("MarketParticipants")]
        [EnableQuery]
        public IActionResult MarketParticipants()
        {
            return Ok(_data.MarketParticipants);
        }

        [HttpGet("MeteringPoints")]
        [EnableQuery]
        public IActionResult MeteringPoints()
        {
            return Ok(_data.MeteringPoints);
        }

        [HttpGet("ChargeInformations")]
        [EnableQuery]
        public IActionResult ChargeInformations()
        {
            return Ok(_data.ChargeInformations);
        }

        [HttpGet("ChargeLinks")]
        [EnableQuery]
        public IActionResult ChargeLinks()
        {
            return Ok(_data.ChargeLinks);
        }

        [HttpGet("DefaultChargeLinks")]
        [EnableQuery]
        public IActionResult DefaultChargeLinks()
        {
            return Ok(_data.DefaultChargeLinks);
        }
    }
}
