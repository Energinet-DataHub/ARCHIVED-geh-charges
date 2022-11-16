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
using GreenEnergyHub.Charges.Application.Charges.Handlers.ChargeInformation;
using GreenEnergyHub.Charges.Application.Charges.Handlers.ChargePrice;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands;
using GreenEnergyHub.Charges.Domain.Dtos.Messages.Command;

namespace GreenEnergyHub.Charges.FunctionHost.Charges.Handlers
{
    public class ChargeIngestionBundleHandler : IChargeIngestionBundleHandler
    {
        private readonly IChargeInformationCommandBundleHandler _chargeInformationCommandBundleHandler;
        private readonly IChargePriceCommandBundleHandler _chargePriceCommandBundleHandler;

        public ChargeIngestionBundleHandler(
            IChargeInformationCommandBundleHandler chargeInformationCommandBundleHandler,
            IChargePriceCommandBundleHandler chargePriceCommandBundleHandler)
        {
            _chargeInformationCommandBundleHandler = chargeInformationCommandBundleHandler;
            _chargePriceCommandBundleHandler = chargePriceCommandBundleHandler;
        }

        public async Task HandleAsync(ChargeCommandBundle chargeCommandBundle)
        {
            switch (chargeCommandBundle)
            {
                case ChargeInformationCommandBundle commandBundle:
                    ChargeCommandNullChecker.ThrowExceptionIfRequiredPropertyIsNull(commandBundle);
                    await _chargeInformationCommandBundleHandler.HandleAsync(commandBundle).ConfigureAwait(false);
                    break;
                case ChargePriceCommandBundle commandBundle:
                    await _chargePriceCommandBundleHandler.HandleAsync(commandBundle).ConfigureAwait(false);
                    break;
            }
        }
    }
}
