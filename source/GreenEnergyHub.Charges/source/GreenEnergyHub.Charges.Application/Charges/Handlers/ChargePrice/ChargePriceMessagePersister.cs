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

using System;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.Events;

namespace GreenEnergyHub.Charges.Application.Charges.Handlers.ChargePrice
{
    public class ChargePriceMessagePersister : IChargePriceMessagePersister
    {
        private readonly IChargeMessageRepository _chargeMessageRepository;

        public ChargePriceMessagePersister(IChargeMessageRepository chargeMessageRepository)
        {
            _chargeMessageRepository = chargeMessageRepository;
        }

        public async Task PersistMessagesAsync(ChargePriceOperationsAcceptedEvent chargePriceOperationsAcceptedEvent)
        {
            ArgumentNullException.ThrowIfNull(chargePriceOperationsAcceptedEvent);

            foreach (var chargePriceOperationDto in chargePriceOperationsAcceptedEvent.Operations)
            {
                var chargeMessage = ChargeMessage.Create(
                    chargePriceOperationDto.SenderProvidedChargeId,
                    chargePriceOperationDto.ChargeType,
                    chargePriceOperationDto.ChargeOwner,
                    chargePriceOperationsAcceptedEvent.Document.Id);
                await _chargeMessageRepository.AddAsync(chargeMessage).ConfigureAwait(false);
            }
        }
    }
}
