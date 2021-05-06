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
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using GreenEnergyHub.Charges.Domain.Common;
using NodaTime;

namespace GreenEnergyHub.Charges.Tests.Builders
{
   public class ChangeOfChargesTransactionBuilder
    {
        private string _mrid;
        private Instant _validityStartDate;
        private string _vatPayer;
        private bool _taxIndicator;
        private int _status;
        private string _owner;

        public ChangeOfChargesTransactionBuilder()
        {
            _mrid = "some mrid";
            _validityStartDate = SystemClock.Instance.GetCurrentInstant()
                .Plus(Duration.FromDays(500));
            _vatPayer = "D02";
            _status = 2; // Addition
            _taxIndicator = false;
            _owner = "owner";
        }

        public ChangeOfChargesTransactionBuilder WithMrid(string mrid)
        {
            _mrid = mrid;
            return this;
        }

        public ChangeOfChargesTransactionBuilder WithValidityStartDateDays(int days)
        {
            _validityStartDate = SystemClock.Instance.GetCurrentInstant()
                .Plus(Duration.FromDays(days));
            return this;
        }

        public ChangeOfChargesTransactionBuilder WithTaxIndicator(bool taxIndicator)
        {
            _taxIndicator = taxIndicator;
            return this;
        }

        public ChangeOfChargesTransactionBuilder WithOwner(string owner)
        {
            _owner = owner;
            return this;
        }

        public ChangeOfChargesTransactionBuilder WithVatPayer(string vatPayer)
        {
            _vatPayer = vatPayer;
            return this;
        }

        public ChangeOfChargesTransactionBuilder WithStatus(int status)
        {
            _status = status;
            return this;
        }

        public ChargeCommand Build()
        {
            return new ("some-correlation-id")
            {
                ChargeDto = new ChargeDto
                {
                    Name = "description",
                    Id = _mrid,
                    Owner = _owner,
                    StartDateTime = _validityStartDate,
                    Points = new List<Point>
                    {
                        new Point { Position = 0, Time = SystemClock.Instance.GetCurrentInstant(), Price = 200m },
                    },
                    Resolution = Resolution.PT1H,
                    Type = ChargeType.Fee,
                    Vat = Vat.Vat25,
                    Description = "LongDescription",
                    Tax = _taxIndicator,
                },
                Document = new Document
                {
                    Id = "id",
                    CorrelationId = "CorrelationId",
                    Type = "type",
                    RequestDate = SystemClock.Instance.GetCurrentInstant(),
                    IndustryClassification = IndustryClassification.Electricity,
                    CreatedDateTime = SystemClock.Instance.GetCurrentInstant(),
                },
                ChargeOperation = new ChargeOperation
                {
                  Id = "id",
                  Status = ChargeEventFunction.Addition,
                  BusinessReasonCode = BusinessReasonCode.UpdateChargeInformation,
                  LastUpdatedBy = "LastUpdatedBy",
                },
            };
        }
    }
}
