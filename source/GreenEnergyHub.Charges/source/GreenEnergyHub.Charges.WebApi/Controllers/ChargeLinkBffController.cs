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

using System.Collections.Generic;
using System.Linq;
using GreenEnergyHub.Charges.QueryApi;
using GreenEnergyHub.Charges.QueryApi.ScaffoldedModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChargeLinkBffController : ControllerBase
    {
        private readonly ILogger<ChargeLinkBffController> _logger;
        private readonly IData _data;

        public ChargeLinkBffController(ILogger<ChargeLinkBffController> logger, IData data)
        {
            _logger = logger;
            _data = data;
        }

        [HttpGet]
        public IEnumerable<object> Get(string meteringPointId)
        {
            _logger.LogCritical("Hey BFF!");

            return _data
                .ChargeLinks
                .ForMeteringPoint(meteringPointId)
                .Select(c => new
                {
                    ChargeId = c.Charge.SenderProvidedChargeId,
                    ChargeType = c.Charge.ChargeType,
                    ChargeOwner = c.Charge.MarketParticipant,
                })
                .ToList();
        }
    }
}
