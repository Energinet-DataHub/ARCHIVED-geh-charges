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
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.ChargeCommands;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using NodaTime;

namespace GreenEnergyHub.Charges.Domain.AvailableChargeData
{
    public class AvailableChargeDataFactory : IAvailableChargeDataFactory
    {
        public AvailableChargeData Create(
            ChargeCommand chargeCommand,
            MarketParticipant recipient,
            Instant requestTime,
            Guid messageHubId)
        {
            var points =
                chargeCommand.ChargeOperation.Points
                    .Select(x => new AvailableChargeDataPoint(x.Position, x.Price))
                    .ToList();

            return new AvailableChargeData(
                recipient.Id,
                recipient.BusinessProcessRole,
                chargeCommand.Document.BusinessReasonCode,
                chargeCommand.ChargeOperation.ChargeId,
                chargeCommand.ChargeOperation.ChargeOwner,
                chargeCommand.ChargeOperation.Type,
                chargeCommand.ChargeOperation.ChargeName,
                chargeCommand.ChargeOperation.ChargeDescription,
                chargeCommand.ChargeOperation.StartDateTime,
                chargeCommand.ChargeOperation.EndDateTime.TimeOrEndDefault(),
                chargeCommand.ChargeOperation.VatClassification,
                chargeCommand.ChargeOperation.TaxIndicator,
                chargeCommand.ChargeOperation.TransparentInvoicing,
                chargeCommand.ChargeOperation.Resolution,
                points,
                requestTime,
                messageHubId);
        }
    }
}
