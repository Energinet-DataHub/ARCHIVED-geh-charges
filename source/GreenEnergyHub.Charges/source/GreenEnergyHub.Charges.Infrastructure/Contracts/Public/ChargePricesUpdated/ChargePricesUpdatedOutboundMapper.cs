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
using Energinet.DataHub.Core.Messaging.Protobuf;
using Google.Protobuf;
using GreenEnergyHub.Charges.Application.ChargePrices;
using GreenEnergyHub.Charges.Application.ChargePrices.Acknowledgement;
using GreenEnergyHub.Charges.Application.Charges.Acknowledgement;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Core.Enumeration;
using GreenEnergyHub.Charges.Infrastructure.Integration.ChargeConfirmation;

namespace GreenEnergyHub.Charges.Infrastructure.Contracts.Public.ChargePricesUpdated
{
    public class ChargePricesUpdatedOutboundMapper : ProtobufOutboundMapper<ChargePricesUpdatedEvent>
    {
        protected override IMessage Convert([NotNull]ChargePricesUpdatedEvent chargePricesUpdatedEvent)
        {
            var chargePricesUpdatedContract = new Integration.ChargeConfirmation.ChargePricesUpdated
            {
                ChargeId = chargePricesUpdatedEvent.ChargeId,
                ChargeOwner = chargePricesUpdatedEvent.ChargeOwner,
                ChargeType = chargePricesUpdatedEvent.ChargeType.Cast<Integration.ChargeConfirmation.ChargePricesUpdated.Types.ChargeType>(),
                UpdatePeriodStartDate = chargePricesUpdatedEvent.UpdatePeriodStartDate.ToTimestamp(),
                UpdatePeriodEndDate = chargePricesUpdatedEvent.UpdatePeriodEndDate.ToTimestamp(),
            };

            foreach (var point in chargePricesUpdatedEvent.Points)
            {
                var mappedPoint = new ChargePrice { Price = point.Price, Time = point.Time.ToTimestamp(), };

                chargePricesUpdatedContract.Points.Add(mappedPoint);
            }

            return chargePricesUpdatedContract;
        }
    }
}
