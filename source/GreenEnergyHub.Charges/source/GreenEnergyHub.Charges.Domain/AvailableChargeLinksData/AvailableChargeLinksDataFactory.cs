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
using System.Linq;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinkCommands;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using NodaTime;

namespace GreenEnergyHub.Charges.Domain.AvailableChargeLinksData
{
    public class AvailableChargeLinksDataFactory : IAvailableChargeLinksDataFactory
    {
        public AvailableChargeLinksData CreateAvailableChargeLinksData(
            ChargeLinkCommand chargeLinkCommand,
            MarketParticipant recipient,
            Instant requestTime,
            Guid messageHubId)
        {
            return new AvailableChargeLinksData(
                recipient.Id,
                recipient.BusinessProcessRole,
                chargeLinkCommand.Document.BusinessReasonCode,
                chargeLinkCommand.ChargeLink.SenderProvidedChargeId,
                chargeLinkCommand.ChargeLink.ChargeOwner,
                chargeLinkCommand.ChargeLink.ChargeType,
                chargeLinkCommand.ChargeLink.MeteringPointId,
                chargeLinkCommand.ChargeLink.Factor,
                chargeLinkCommand.ChargeLink.StartDateTime,
                chargeLinkCommand.ChargeLink.EndDateTime.GetValueOrDefault(),
                requestTime,
                messageHubId);
        }
    }
}
