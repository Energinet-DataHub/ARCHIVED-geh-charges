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
using System.Linq;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.Events;

namespace GreenEnergyHub.Charges.Application.Charges.Handlers.ChargeInformation
{
    public class ChargeInformationMessagePersister : IChargeInformationMessagePersister
    {
        private readonly IChargeMessageRepository _chargeMessageRepository;

        public ChargeInformationMessagePersister(IChargeMessageRepository chargeMessageRepository)
        {
            _chargeMessageRepository = chargeMessageRepository;
        }

        public async Task PersistMessageAsync(ChargeInformationOperationsAcceptedEvent chargeInformationOperationsAcceptedEvent)
        {
            ArgumentNullException.ThrowIfNull(chargeInformationOperationsAcceptedEvent);

            // Charge id, type and owner is the same for all operations at this point
            var chargeInformationOperationDto = chargeInformationOperationsAcceptedEvent.Operations.First();
            var chargeMessage = ChargeMessage.Create(
                chargeInformationOperationDto.SenderProvidedChargeId,
                chargeInformationOperationDto.ChargeType,
                chargeInformationOperationDto.ChargeOwner,
                chargeInformationOperationsAcceptedEvent.Document.Id,
                chargeInformationOperationsAcceptedEvent.Document.Type,
                chargeInformationOperationsAcceptedEvent.Document.RequestDate);
            await _chargeMessageRepository.AddAsync(chargeMessage).ConfigureAwait(false);
        }
    }
}
