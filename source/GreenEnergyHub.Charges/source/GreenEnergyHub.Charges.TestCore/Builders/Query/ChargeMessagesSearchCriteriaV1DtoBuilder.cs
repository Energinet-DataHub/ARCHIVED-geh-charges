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
using Energinet.DataHub.Charges.Contracts.ChargeMessage;

namespace GreenEnergyHub.Charges.TestCore.Builders.Query
{
    public class ChargeMessagesSearchCriteriaV1DtoBuilder
    {
        private Guid _chargeId = Guid.NewGuid();
        private DateTimeOffset _fromDateTime = DateTimeOffset.Now.AddYears(-3);
        private DateTimeOffset _toDateTime = DateTimeOffset.Now.AddYears(3);
        private ChargeMessageSortColumnName _chargeMessageSortColumnName = ChargeMessageSortColumnName.MessageDateTime;
        private bool _isDescending;
        private int _skip;
        private int _take = 10;

        public ChargeMessagesSearchCriteriaV1DtoBuilder WithChargeId(Guid chargeId)
        {
            _chargeId = chargeId;
            return this;
        }

        public ChargeMessagesSearchCriteriaV1DtoBuilder WithFromDateTime(DateTimeOffset fromDateTime)
        {
            _fromDateTime = fromDateTime;
            return this;
        }

        public ChargeMessagesSearchCriteriaV1DtoBuilder WithToDateTime(DateTimeOffset toDateTime)
        {
            _toDateTime = toDateTime;
            return this;
        }

        public ChargeMessagesSearchCriteriaV1DtoBuilder WithSortColumnName(ChargeMessageSortColumnName chargeMessageSortColumnName)
        {
            _chargeMessageSortColumnName = chargeMessageSortColumnName;
            return this;
        }

        public ChargeMessagesSearchCriteriaV1DtoBuilder WithIsDescending(bool isDescending)
        {
            _isDescending = isDescending;
            return this;
        }

        public ChargeMessagesSearchCriteriaV1DtoBuilder WithSkip(int skip)
        {
            _skip = skip;
            return this;
        }

        public ChargeMessagesSearchCriteriaV1DtoBuilder WithTake(int take)
        {
            _take = take;
            return this;
        }

        public ChargeMessagesSearchCriteriaV1Dto Build()
        {
            return new ChargeMessagesSearchCriteriaV1Dto(
                _chargeId,
                _fromDateTime,
                _toDateTime,
                _chargeMessageSortColumnName,
                _isDescending,
                _skip,
                _take);
        }
    }
}
