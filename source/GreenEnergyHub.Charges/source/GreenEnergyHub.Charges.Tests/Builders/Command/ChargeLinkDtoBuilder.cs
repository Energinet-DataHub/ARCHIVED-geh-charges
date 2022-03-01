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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands;
using NodaTime;

namespace GreenEnergyHub.Charges.Tests.Builders.Command
{
    public class ChargeLinkDtoBuilder
    {
        private Instant _startDate = SystemClock.Instance.GetCurrentInstant();
        private Instant? _endDate = Instant.MaxValue;
        private int _factor = 1;
        private string _senderProvidedChargeId = string.Empty;
        private ChargeType _chargeType;
        private string _chargeOwner = string.Empty;

        public ChargeLinkDtoBuilder WithStartDate(Instant startDate)
        {
            _startDate = startDate;
            return this;
        }

        public ChargeLinkDtoBuilder WithEndDate(Instant? endDate)
        {
            _endDate = endDate;
            return this;
        }

        public ChargeLinkDto Build()
        {
            return new ChargeLinkDto
            {
                StartDateTime = _startDate,
                EndDateTime = _endDate,
                ChargeType = _chargeType,
                ChargeOwner = _chargeOwner,
                SenderProvidedChargeId = _senderProvidedChargeId,
                Factor = _factor,
                OperationId = Guid.NewGuid().ToString("N"),
            };
        }

        public ChargeLinkDtoBuilder WithCharge(string senderProvidedChargeId, ChargeType chargeType, string chargeOwner)
        {
            _senderProvidedChargeId = senderProvidedChargeId;
            _chargeType = chargeType;
            _chargeOwner = chargeOwner;
            return this;
        }
    }
}
