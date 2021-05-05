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
using System.Data;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using GreenEnergyHub.Charges.Domain.Common;
using NodaTime;
using MarketParticipant = GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction.MarketParticipant;
using MarketParticipantRole = GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction.MarketParticipantRole;

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
            return new ()
            {
                Charge = new ChargeNew
                {
                    Name = "description",
                    Id = _mrid,
                    Owner = _owner,
                    Points = new List<Point>
                    {
                        new Point { Position = 0, Time = SystemClock.Instance.GetCurrentInstant(), PriceAmount = 200m },
                    },
                    Resolution = "Resolution",
                    Type = "Type",
                    Vat = _vatPayer,
                    Description = "LongDescription",
                    RequestDate = SystemClock.Instance.GetCurrentInstant(),
                    Tax = _taxIndicator,
                },
                Document = new Document
                {
                    Id = "id",
                    Type = "type",
                    IndustryClassification = IndustryClassification.Electricity,
                    BusinessReasonCode = BusinessReasonCode.D18,
                    CreatedDateTime = SystemClock.Instance.GetCurrentInstant(),
                },
                ChargeEvent = new ChargeEvent
                {
                  Id = "id",
                  Status = ChargeEventFunction.Addition,
                  CorrelationId = "CorrelationId",
                  LastUpdatedBy = "LastUpdatedBy",
                  StartDateTime = _validityStartDate,
                },
            };
        }
    }
}
