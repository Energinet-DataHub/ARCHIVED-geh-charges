﻿// Copyright 2020 Energinet DataHub A/S
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
using Energinet.DataHub.Charges.Contracts.ChargePrice;

namespace GreenEnergyHub.Charges.TestCore.Builders.Query
{
    public class ChargePricesSearchCriteriaV1DtoBuilder
    {
        private Guid _chargeId = Guid.NewGuid();
        private DateTimeOffset _fromDateTime = DateTimeOffset.MinValue;
        private DateTimeOffset _toDateTime = DateTimeOffset.MaxValue;
        private ChargePriceSortColumnName _chargePriceSortColumnName = ChargePriceSortColumnName.FromDateTime;
        private bool _isDescending;
        private int _skip;
        private int _take = 10;

        public ChargePricesSearchCriteriaV1DtoBuilder WithChargeId(Guid chargeId)
        {
            _chargeId = chargeId;
            return this;
        }

        public ChargePricesSearchCriteriaV1DtoBuilder WithFromDateTime(DateTimeOffset fromDateTime)
        {
            _fromDateTime = fromDateTime;
            return this;
        }

        public ChargePricesSearchCriteriaV1DtoBuilder WithToDateTime(DateTimeOffset toDateTime)
        {
            _toDateTime = toDateTime;
            return this;
        }

        public ChargePricesSearchCriteriaV1DtoBuilder WithSortColumnName(ChargePriceSortColumnName chargePriceSortColumnName)
        {
            _chargePriceSortColumnName = chargePriceSortColumnName;
            return this;
        }

        public ChargePricesSearchCriteriaV1DtoBuilder WithIsDescending(bool isDescending)
        {
            _isDescending = isDescending;
            return this;
        }

        public ChargePricesSearchCriteriaV1DtoBuilder WithSkip(int skip)
        {
            _skip = skip;
            return this;
        }

        public ChargePricesSearchCriteriaV1DtoBuilder WithTake(int take)
        {
            _take = take;
            return this;
        }

        public ChargePricesSearchCriteriaV1Dto Build()
        {
            return new ChargePricesSearchCriteriaV1Dto(
                _chargeId,
                _fromDateTime,
                _toDateTime,
                _chargePriceSortColumnName,
                _isDescending,
                _skip,
                _take);
        }
    }
}
