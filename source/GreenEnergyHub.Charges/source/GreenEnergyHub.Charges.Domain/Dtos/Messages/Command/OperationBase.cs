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

using GreenEnergyHub.Charges.Domain.Charges;
using NodaTime;

namespace GreenEnergyHub.Charges.Domain.Dtos.Messages.Command
{
    public abstract class OperationBase
    {
        protected OperationBase(
            string operationId,
            ChargeType chargeType,
            string senderProvidedChargeId,
            string chargeOwner,
            Instant startDate,
            Instant? endDate)
        {
            OperationId = operationId;
            ChargeType = chargeType;
            SenderProvidedChargeId = senderProvidedChargeId;
            ChargeOwner = chargeOwner;
            StartDate = startDate;
            EndDate = endDate;
        }

        /// <summary>
        /// Contains a unique ID for the specific Charge OperationId, provided by the sender.
        /// </summary>
        public string OperationId { get; }

        /// <summary>
        /// Type of charge, i.e. Tariff, Fee or Subscription
        /// </summary>
        public ChargeType ChargeType { get; }

        /// <summary>
        ///  Charge Owner, e.g. the GLN or EIC identification number.
        /// </summary>
        public string ChargeOwner { get; }

        /// <summary>
        /// Unique ID of a charge (Note, unique per market participants).
        /// Example: EA-001
        /// </summary>
        public string SenderProvidedChargeId { get; }

        /// <summary>
        /// Valid from, of a charge operation. Also known as Effective Date.
        /// </summary>
        public Instant StartDate { get; }

        /// <summary>
        /// Valid to, of a charge operation. Also known as Termination Date.
        /// </summary>
        public Instant? EndDate { get; }
    }
}
