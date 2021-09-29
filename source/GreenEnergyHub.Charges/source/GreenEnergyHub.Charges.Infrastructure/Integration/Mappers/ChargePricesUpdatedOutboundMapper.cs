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
using Google.Protobuf;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Core.Enumeration;
using GreenEnergyHub.Charges.Domain.Charges.Acknowledgements;
using GreenEnergyHub.Charges.Infrastructure.Integration.ChargeConfirmation;
using GreenEnergyHub.Messaging.Protobuf;

namespace GreenEnergyHub.Charges.Infrastructure.Integration.Mappers
{
    public class ChargePricesUpdatedOutboundMapper : ProtobufOutboundMapper<Domain.Charges.Acknowledgements.ChargePricesUpdated>
    {
        protected override IMessage Convert([NotNull]GreenEnergyHub.Charges.Domain.Charges.Acknowledgements.ChargePricesUpdated chargePricesUpdated)
        {
            var chargePricesUpdatedContract = new ChargeConfirmation.ChargePricesUpdated
            {
                ChargeId = chargePricesUpdated.ChargeId,
                ChargeOwner = chargePricesUpdated.ChargeOwner,
                ChargeType = chargePricesUpdated.ChargeType.Cast<ChargeType>(),
                UpdatePeriodStartDate = chargePricesUpdated.UpdatePeriodStartDate.ToTimestamp(),
                UpdatePeriodEndDate = chargePricesUpdated.UpdatePeriodEndDate.ToTimestamp(),
            };

            foreach (var point in chargePricesUpdated.Points)
            {
                var mappedPoint = new ChargePrice { Price = point.Price, Time = point.Time.ToTimestamp(), };

                chargePricesUpdatedContract.Points.Add(mappedPoint);
            }

            return chargePricesUpdatedContract;
        }
    }
}
