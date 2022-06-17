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
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands
{
    public static class ChargeCommandNullChecker
    {
        public static void ThrowExceptionIfRequiredPropertyIsNull(ChargeBundleDto chargeBundleDto)
        {
            ArgumentNullException.ThrowIfNull(chargeBundleDto);
            var document = chargeBundleDto.Document;
            CheckDocument(document);

            var chargeCommands = chargeBundleDto.ChargeCommands;
            ArgumentNullException.ThrowIfNull(chargeCommands);

            foreach (var chargeCommand in chargeCommands)
            {
                ArgumentNullException.ThrowIfNull(chargeCommand);

                foreach (var chargeDto in chargeCommand.ChargeOperations)
                {
                    CheckChargeOperation(chargeDto, chargeCommand.Document.BusinessReasonCode);
                }
            }
        }

        private static void CheckChargeOperation(
            ChargeOperationDto chargeOperationDto,
            BusinessReasonCode businessReasonCode)
        {
            if (chargeOperationDto == null) throw new ArgumentNullException(nameof(chargeOperationDto));

            if (businessReasonCode != BusinessReasonCode.UpdateChargeInformation)
                return;

            if (string.IsNullOrWhiteSpace(chargeOperationDto.ChargeName))
                throw new ArgumentException(chargeOperationDto.ChargeName);
            if (string.IsNullOrWhiteSpace(chargeOperationDto.ChargeDescription))
                throw new ArgumentException(chargeOperationDto.ChargeDescription);
        }

        private static void CheckDocument(DocumentDto document)
        {
            ArgumentNullException.ThrowIfNull(document);
            if (string.IsNullOrWhiteSpace(document.Id)) throw new ArgumentException(document.Id);
            ArgumentNullException.ThrowIfNull(document.Recipient);
            ArgumentNullException.ThrowIfNull(document.Sender);
        }
    }
}
