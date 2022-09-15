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
using GreenEnergyHub.Charges.Domain.Dtos.Messages.Command;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands
{
    public static class ChargeCommandNullChecker
    {
        public static void ThrowExceptionIfRequiredPropertyIsNull(ChargeInformationCommandBundle chargeInformationCommandBundle)
        {
            ArgumentNullException.ThrowIfNull(chargeInformationCommandBundle);
            var document = chargeInformationCommandBundle.Document;

            CheckDocument(document);

            var commands = chargeInformationCommandBundle.Commands;
            ArgumentNullException.ThrowIfNull(commands);

            foreach (var command in commands)
            {
                ArgumentNullException.ThrowIfNull(command);
                foreach (var operation in command.Operations)
                {
                    CheckOperation(operation);
                }
            }
        }

        private static void CheckOperation(ChargeOperationDto operationDto)
        {
            if (operationDto == null) throw new ArgumentNullException(nameof(operationDto));
        }

        private static void CheckDocument(DocumentDto document)
        {
            ArgumentNullException.ThrowIfNull(document);
            if (string.IsNullOrWhiteSpace(document.Id)) throw new ArgumentException(document.Id);
            ArgumentNullException.ThrowIfNull(document.BusinessReasonCode);
            ArgumentNullException.ThrowIfNull(document.Recipient);
            ArgumentNullException.ThrowIfNull(document.Sender);
        }
    }
}
