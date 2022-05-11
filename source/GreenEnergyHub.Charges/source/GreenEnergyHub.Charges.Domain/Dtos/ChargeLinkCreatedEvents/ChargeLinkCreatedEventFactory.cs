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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands;

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargeLinkCreatedEvents
{
    public class ChargeLinkCreatedEventFactory : IChargeLinkCreatedEventFactory
    {
        public IReadOnlyCollection<ChargeLinkCreatedEvent> CreateEvents(ChargeLinksCommand command)
        {
            return command.ChargeLinksOperations.Select(ChargeLinkCreatedEvent).ToList();
        }

        private static ChargeLinkCreatedEvent ChargeLinkCreatedEvent(ChargeLinkDto chargeLinkDto)
        {
            return new ChargeLinkCreatedEvent(
                chargeLinkDto.OperationId,
                chargeLinkDto.MeteringPointId,
                chargeLinkDto.SenderProvidedChargeId,
                chargeLinkDto.ChargeType,
                chargeLinkDto.ChargeOwner,
                ChargeLinkPeriod(chargeLinkDto));
        }

        private static ChargeLinkPeriod ChargeLinkPeriod(ChargeLinkDto chargeLinkDto)
        {
            return new ChargeLinkPeriod(
                chargeLinkDto.StartDateTime,
                chargeLinkDto.EndDateTime.TimeOrEndDefault(),
                chargeLinkDto.Factor);
        }
    }
}
