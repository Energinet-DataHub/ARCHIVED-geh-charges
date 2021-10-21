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
using GreenEnergyHub.Charges.Domain.Charges;
using NodaTime;

namespace GreenEnergyHub.Charges.Domain.AvailableChargeData
{
    public class AvailableChargeData
    {
        public AvailableChargeData(
            VatClassification vatClassification,
            bool taxIndicator,
            bool transparentInvoicing,
            Instant requestTime,
            Guid availableDataReferenceId)
        {
            Id = Guid.NewGuid();
            VatClassification = vatClassification;
            TaxIndicator = taxIndicator;
            TransparentInvoicing = transparentInvoicing;
            RequestTime = requestTime;
            AvailableDataReferenceId = availableDataReferenceId;
        }

        public Guid Id { get; }

        public VatClassification VatClassification { get; }

        public bool TaxIndicator { get; }

        public bool TransparentInvoicing { get; }

        public Instant RequestTime { get; }

        public Guid AvailableDataReferenceId { get; }
    }
}
