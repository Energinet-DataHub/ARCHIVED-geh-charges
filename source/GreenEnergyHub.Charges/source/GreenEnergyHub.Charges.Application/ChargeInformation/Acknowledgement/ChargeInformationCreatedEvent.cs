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

using Energinet.DataHub.Core.Messaging.MessageTypes.Common;
using GreenEnergyHub.Charges.Domain.Common;
using GreenEnergyHub.Charges.Domain.Dtos.Messages;
using NodaTime;

namespace GreenEnergyHub.Charges.Application.ChargeInformation.Acknowledgement
{
    public class ChargeInformationCreatedEvent : IMessage
    {
        public ChargeInformationCreatedEvent(
            string chargeId,
            ChargeType chargeType,
            string chargeOwner,
            string currency,
            Resolution resolution,
            bool taxIndicator,
            Instant startDateTime,
            Instant endDateTime)
        {
            ChargeId = chargeId;
            ChargeType = chargeType;
            ChargeOwner = chargeOwner;
            Currency = currency;
            Resolution = resolution;
            TaxIndicator = taxIndicator;
            StartDateTime = startDateTime;
            EndDateTime = endDateTime;
            Transaction = Transaction.NewTransaction();
        }

        public string ChargeId { get; }

        public ChargeType ChargeType { get; }

        public string ChargeOwner { get; }

        public string Currency { get; }

        public Resolution Resolution { get; }

        public bool TaxIndicator { get; }

        public Instant StartDateTime { get; }

        public Instant EndDateTime { get; }

        public Transaction Transaction { get; set; }
    }
}
