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
using GreenEnergyHub.Charges.TestCore.TestHelpers;
using NodaTime;

namespace GreenEnergyHub.Charges.TestCore.Builders.Command
{
    public class ChargeHistoryBuilder
    {
        private static string _senderProvidedChargeId = Guid.NewGuid().ToString();
        private static ChargeType _chargeType = ChargeType.Fee;
        private static string _owner = "owner";
        private static string _name = "name";
        private static string _description = "description";
        private static Resolution _resolution = Resolution.P1M;
        private static TaxIndicator _taxIndicator = TaxIndicator.NoTax;
        private static TransparentInvoicing _transparentInvoicing = TransparentInvoicing.NonTransparent;
        private static VatClassification _vatClassification = VatClassification.NoVat;
        private static Instant _startDateTime = InstantHelper.GetTodayAtMidnightUtc();
        private static Instant? _endDateTime = InstantHelper.GetEndDefault();
        private static Instant _acceptedDateTime = SystemClock.Instance.GetCurrentInstant();

        public ChargeHistoryBuilder WithSenderProvidedChargeId(string senderProvidedChargeId)
        {
            _senderProvidedChargeId = senderProvidedChargeId;
            return this;
        }

        public ChargeHistoryBuilder WithChargeType(ChargeType chargeType)
        {
            _chargeType = chargeType;
            return this;
        }

        public ChargeHistoryBuilder WithOwner(string owner)
        {
            _owner = owner;
            return this;
        }

        public ChargeHistoryBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public ChargeHistoryBuilder WithTaxIndicator(TaxIndicator taxIndicator)
        {
            _taxIndicator = taxIndicator;
            return this;
        }

        public ChargeHistoryBuilder WithTransparentInvoicing(TransparentInvoicing transparentInvoicing)
        {
            _transparentInvoicing = transparentInvoicing;
            return this;
        }

        public ChargeHistoryBuilder WithStartDateTime(Instant startDateTime)
        {
            _startDateTime = startDateTime;
            return this;
        }

        public ChargeHistoryBuilder WithEndDateTime(Instant endDateTime)
        {
            _endDateTime = endDateTime;
            return this;
        }

        public ChargeHistoryBuilder WithAcceptedDateTime(Instant acceptedDateTime)
        {
            _acceptedDateTime = acceptedDateTime;
            return this;
        }

        public ChargeHistory Build()
        {
            return ChargeHistory.Create(
                _senderProvidedChargeId,
                _chargeType,
                _owner,
                _name,
                _description,
                _resolution,
                _taxIndicator,
                _transparentInvoicing,
                _vatClassification,
                _startDateTime,
                _endDateTime,
                _acceptedDateTime);
        }
    }
}
