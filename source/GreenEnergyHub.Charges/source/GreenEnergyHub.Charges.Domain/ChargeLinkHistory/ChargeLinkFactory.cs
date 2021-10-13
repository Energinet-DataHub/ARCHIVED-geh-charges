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
            ChargeLinkCommandAcceptedEvent chargeLinkCommandAcceptedEvent, MarketParticipant marketParticipant)
        {
            return new ChargeLinkHistory(
                marketParticipant.Id,
                MarketParticipantRole.Unknown, // TODO where
                chargeLinkCommandAcceptedEvent.Document.BusinessReasonCode,
                "unknown", // TODO where
                chargeLinkCommandAcceptedEvent.ChargeLink.MeteringPointId,
                chargeLinkCommandAcceptedEvent.Document.Sender.Id,
                chargeLinkCommandAcceptedEvent.ChargeLink.Factor,
                chargeLinkCommandAcceptedEvent.ChargeLink.ChargeType,
                chargeLinkCommandAcceptedEvent.ChargeLink.StartDateTime,
                chargeLinkCommandAcceptedEvent.ChargeLink.EndDateTime.GetValueOrDefault(), // TODO nullable?
                Guid.NewGuid()); // TODO to be parsed on from somewhere?
        }
    }
}
