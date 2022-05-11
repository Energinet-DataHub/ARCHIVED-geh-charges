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
using Energinet.DataHub.Core.Messaging.MessageTypes.Common;
using GreenEnergyHub.Charges.Domain.ChargePrices;
using GreenEnergyHub.Charges.Domain.Common;
using GreenEnergyHub.Charges.Domain.Dtos.Messages;
using NodaTime;

namespace GreenEnergyHub.Charges.Application.ChargePrices.Acknowledgement
{
    public class ChargePricesUpdatedEvent : IMessage
    {
        public ChargePricesUpdatedEvent(
            string chargeInformationId,
            ChargeType chargeType,
            string chargeOwner,
            Instant updatePeriodStartDate,
            Instant updatePeriodEndDate,
            List<Point> points)
        {
            ChargeInformationId = chargeInformationId;
            ChargeType = chargeType;
            ChargeOwner = chargeOwner;
            UpdatePeriodStartDate = updatePeriodStartDate;
            UpdatePeriodEndDate = updatePeriodEndDate;
            Points = points;
            Transaction = Transaction.NewTransaction();
        }

        public string ChargeInformationId { get; }

        public ChargeType ChargeType { get; }

        public string ChargeOwner { get; }

        public Instant UpdatePeriodStartDate { get; }

        public Instant UpdatePeriodEndDate { get; }

        public List<Point> Points { get; }

        public Transaction Transaction { get; set; }
    }
}
