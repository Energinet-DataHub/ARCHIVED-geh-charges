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

using System;
using System.Collections.Generic;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.MarketParticipants;

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands
{
    public static class ChargeCommandNullChecker
    {
        public static void ThrowExceptionIfRequiredPropertyIsNull(List<ChargeCommand> chargeCommands)
        {
            CheckListOfChargeCommands(chargeCommands);

            foreach (var chargeCommand in chargeCommands)
            {
                CheckChargeCommand(chargeCommand);

                CheckDocument(chargeCommand.Document);

                foreach (var chargeDto in chargeCommand.ChargeOperations)
                {
                    CheckChargeOperation(chargeDto, chargeCommand.Document.BusinessReasonCode);
                }
            }
        }

        private static void CheckListOfChargeCommands(List<ChargeCommand> chargeCommands)
        {
            if (chargeCommands == null) throw new ArgumentNullException(nameof(chargeCommands));
        }

        private static void CheckChargeCommand(ChargeCommand chargeCommand)
        {
            if (chargeCommand == null) throw new ArgumentNullException(nameof(chargeCommand));
        }

        private static void CheckChargeOperation(ChargeOperation chargeOperation, BusinessReasonCode businessReasonCode)
        {
            if (chargeOperation == null) throw new ArgumentNullException(nameof(chargeOperation));

            if (businessReasonCode != BusinessReasonCode.UpdateChargeInformation) return;
            if (chargeOperation is not ChargeInformationDto chargeInformationDto) return;

            if (string.IsNullOrWhiteSpace(chargeInformationDto.ChargeName))
                throw new ArgumentException(chargeInformationDto.ChargeName);
            if (string.IsNullOrWhiteSpace(chargeInformationDto.ChargeDescription))
                throw new ArgumentException(chargeInformationDto.ChargeDescription);
        }

        private static void CheckDocument(DocumentDto document)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));
            if (string.IsNullOrWhiteSpace(document.Id)) throw new ArgumentException(document.Id);
            CheckMarketParticipant(document.Recipient);
            CheckMarketParticipant(document.Sender);
        }

        private static void CheckMarketParticipant(MarketParticipantDto marketParticipant)
        {
            if (marketParticipant == null) throw new ArgumentNullException(nameof(marketParticipant));
        }
    }
}
