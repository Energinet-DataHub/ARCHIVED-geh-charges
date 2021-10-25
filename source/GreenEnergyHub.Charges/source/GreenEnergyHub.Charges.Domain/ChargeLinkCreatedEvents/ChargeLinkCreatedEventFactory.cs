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

using System.Diagnostics.CodeAnalysis;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.ChargeLinkCommandAcceptedEvents;
using GreenEnergyHub.Charges.Domain.ChargeLinkCommands;

namespace GreenEnergyHub.Charges.Domain.ChargeLinkCreatedEvents
{
    public class ChargeLinkCreatedEventFactory : IChargeLinkCreatedEventFactory
    {
        public ChargeLinkCreatedEvent CreateEvent([NotNull] ChargeLinkCommand command)
        {
            return new ChargeLinkCreatedEvent(
                command.ChargeLink.OperationId,
                command.ChargeLink.MeteringPointId,
                command.ChargeLink.SenderProvidedChargeId,
                command.ChargeLink.ChargeType,
                command.ChargeLink.ChargeOwner,
                new ChargeLinkPeriod(
                    command.ChargeLink.StartDateTime,
                    command.ChargeLink.EndDateTime.TimeOrEndDefault(),
                    command.ChargeLink.Factor));
        }
    }
}
