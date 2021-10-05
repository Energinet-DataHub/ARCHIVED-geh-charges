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
using GreenEnergyHub.Charges.Application.Charges.Acknowledgement;
using GreenEnergyHub.Charges.Domain.ChargeCommandAcceptedEvents;
using GreenEnergyHub.Charges.Domain.Charges.Acknowledgements;

namespace GreenEnergyHub.Charges.Application.Charges.Factories
{
    public class ChargePricesUpdatedEventFactory : IChargePricesUpdatedEventFactory
    {
        public ChargePricesUpdatedEvent Create([NotNull] ChargeCommandAcceptedEvent chargeCommandAcceptedEvent)
        {
            return new ChargePricesUpdatedEvent(
                chargeCommandAcceptedEvent.Command.ChargeOperation.ChargeId,
                chargeCommandAcceptedEvent.Command.ChargeOperation.Type,
                chargeCommandAcceptedEvent.Command.ChargeOperation.ChargeOwner,
                chargeCommandAcceptedEvent.Command.ChargeOperation.StartDateTime,
                chargeCommandAcceptedEvent.Command.ChargeOperation.EndDateTime.GetValueOrDefault(),
                chargeCommandAcceptedEvent.Command.ChargeOperation.Points,
                chargeCommandAcceptedEvent.CorrelationId);
        }
    }
}
