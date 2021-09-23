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

using System.Diagnostics.CodeAnalysis;
using GreenEnergyHub.Charges.Domain.ChargeLinkCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.ChargeLinkCommands;

namespace GreenEnergyHub.Charges.Domain.ChargeLinkCommandAcceptedEvents
{
    public class ChargeLinkCommandAcceptedEventFactory : IChargeLinkCommandAcceptedEventFactory
    {
        public ChargeLinkCommandAcceptedEvent Create([NotNull] ChargeLinkCommandReceivedEvent chargeCommandLinkReceivedEvent)
        {
            return new ChargeLinkCommandAcceptedEvent(chargeCommandLinkReceivedEvent.CorrelationId)
            {
                Document = chargeCommandLinkReceivedEvent.ChargeLinkCommand.Document,
                ChargeLink = chargeCommandLinkReceivedEvent.ChargeLinkCommand.ChargeLink,
                Transaction = chargeCommandLinkReceivedEvent.Transaction,
            };
        }

        public ChargeLinkCommandAcceptedEvent Create([NotNull] ChargeLinkCommand chargeLinkCommand, string correlationId)
        {
            return new ChargeLinkCommandAcceptedEvent(correlationId)
            {
                Document = chargeLinkCommand.Document,
                ChargeLink = chargeLinkCommand.ChargeLink,
            };
        }
    }
}
