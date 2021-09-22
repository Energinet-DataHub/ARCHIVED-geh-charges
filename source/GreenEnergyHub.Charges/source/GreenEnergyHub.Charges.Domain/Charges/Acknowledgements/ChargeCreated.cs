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

using GreenEnergyHub.Charges.Domain.Messages;
using GreenEnergyHub.Messaging.MessageTypes.Common;

namespace GreenEnergyHub.Charges.Domain.Charges.Acknowledgements
{
    public class ChargeCreated : IMessage
    {
        public ChargeCreated(
            string chargeId,
            ChargeType chargeType,
            string chargeOwner,
            string currency,
            Resolution resolution,
            bool taxIndicator,
            Period chargePeriod,
            string correlationId)
        {
            ChargeId = chargeId;
            ChargeType = chargeType;
            ChargeOwner = chargeOwner;
            Currency = currency;
            Resolution = resolution;
            TaxIndicator = taxIndicator;
            ChargePeriod = chargePeriod;
            Transaction = Transaction.NewTransaction();
            CorrelationId = correlationId;
        }

        public string ChargeId { get; }

        public ChargeType ChargeType { get; }

        public string ChargeOwner { get; }

        public string Currency { get; }

        public Resolution Resolution { get; }

        public bool TaxIndicator { get; }

        public Period ChargePeriod { get; }

        public Transaction Transaction { get; set; }

        public string CorrelationId { get; }
    }
}
