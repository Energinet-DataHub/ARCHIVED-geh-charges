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
using GreenEnergyHub.Charges.Domain.ChargeLinkCommandAcceptedEvents;
using GreenEnergyHub.Charges.Domain.MarketParticipants;

namespace GreenEnergyHub.Charges.Domain.ChargeLinkHistory
{
    public class ChargeLinkFactory : IChargeLinkFactory
    {
        public ChargeLinkHistory MapChargeLinkCommandAcceptedEvent(
            ChargeLinkCommandAcceptedEvent chargeLinkCommandAcceptedEvent,
            MarketParticipant marketParticipant,
            Guid messageHubId)
        {
            return new ChargeLinkHistory(
                marketParticipant.Id,
                marketParticipant.BusinessProcessRole,
                chargeLinkCommandAcceptedEvent.Document.BusinessReasonCode,
                chargeLinkCommandAcceptedEvent.ChargeLink.SenderProvidedChargeId,
                chargeLinkCommandAcceptedEvent.Document.Sender.Id,
                chargeLinkCommandAcceptedEvent.ChargeLink.ChargeType,
                chargeLinkCommandAcceptedEvent.ChargeLink.MeteringPointId,
                chargeLinkCommandAcceptedEvent.ChargeLink.Factor,
                chargeLinkCommandAcceptedEvent.ChargeLink.StartDateTime,
                chargeLinkCommandAcceptedEvent.ChargeLink.EndDateTime.GetValueOrDefault(),
                messageHubId);
        }
    }
}
