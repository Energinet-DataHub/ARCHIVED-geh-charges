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
using Energinet.DataHub.Charges.Contracts.Charge;
using GreenEnergyHub.Charges.Application.Charges.Handlers.ChargeInformation;
using GreenEnergyHub.Charges.QueryApi;
using GreenEnergyHub.Charges.QueryApi.QueryServices;
using GreenEnergyHub.Charges.QueryApi.Validation;
using GreenEnergyHub.Charges.WebApi.Factories;
using Microsoft.AspNetCore.Mvc;

namespace GreenEnergyHub.Charges.WebApi.Controllers.V1
{
    [ApiController]
    [ApiVersion(Version1)]
    [Route("v{version:apiVersion}/[controller]")]
    public class ChargesController : ControllerBase
    {
        public const string Version1 = "1.0";
        private readonly IChargesQueryService _chargesQueryService;
        private readonly IChargeInformationCommandHandler _chargeInformationCommandHandler;
        private readonly IChargeInformationCommandFactory _chargeInformationCommandFactory;

        public ChargesController(IChargesQueryService chargesQueryService, IChargeInformationCommandHandler chargeInformationCommandHandler, IChargeInformationCommandFactory chargeInformationCommandFactory)
        {
            _chargesQueryService = chargesQueryService;
            _chargeInformationCommandHandler = chargeInformationCommandHandler;
            _chargeInformationCommandFactory = chargeInformationCommandFactory;
        }

        /// <summary>
        /// Returns all charges based on the search criteria
        /// </summary>
        /// <returns>Charges data or "400 Bad request"</returns>
        [HttpPost("SearchAsync")]
        [MapToApiVersion(Version1)]
        public async Task<IActionResult> SearchAsync(ChargeSearchCriteriaV1Dto searchCriteria)
        {
            var isValid = ChargeSearchCriteriaValidator.Validate(searchCriteria);
            if (!isValid)
                return BadRequest("SearchAsync criteria is not valid.");

            var charges = await _chargesQueryService.SearchAsync(searchCriteria).ConfigureAwait(false);

            return Ok(charges);
        }

        /// <summary>
        /// Sends a 'ChargeInformationCommand'
        /// </summary>
        /// <returns>"200 OK"</returns>
        [HttpPost("CreateAsync")]
        [MapToApiVersion(Version1)]
        public async Task<IActionResult> CreateAsync([FromBody] CreateChargeInformationV1Dto chargeInformation)
        {
            var command = _chargeInformationCommandFactory.Create(chargeInformation);
            await _chargeInformationCommandHandler.HandleAsync(command).ConfigureAwait(false);

            return Ok();
        }
    }
}
