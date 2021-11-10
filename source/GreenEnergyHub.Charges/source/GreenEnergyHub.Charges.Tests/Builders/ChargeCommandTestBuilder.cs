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
using GreenEnergyHub.Charges.Domain.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Domain.SharedDtos;
using NodaTime;

namespace GreenEnergyHub.Charges.Tests.Builders
{
   public class ChargeCommandTestBuilder
    {
        private string _mrid;
        private Instant _validityStartDate;
        private string _vatPayer;
        private bool _taxIndicator;
        private int _status;
        private string _owner;

        public ChargeCommandTestBuilder()
        {
            _mrid = "some mrid";
            _validityStartDate = SystemClock.Instance.GetCurrentInstant()
                .Plus(Duration.FromDays(500));
            _vatPayer = "D02";
            _status = 2; // Create
            _taxIndicator = false;
            _owner = "owner";
        }

        public ChargeCommandTestBuilder WithMrid(string mrid)
        {
            _mrid = mrid;
            return this;
        }

        public ChargeCommandTestBuilder WithValidityStartDateDays(int days)
        {
            _validityStartDate = SystemClock.Instance.GetCurrentInstant()
                .Plus(Duration.FromDays(days));
            return this;
        }

        public ChargeCommandTestBuilder WithTaxIndicator(bool taxIndicator)
        {
            _taxIndicator = taxIndicator;
            return this;
        }

        public ChargeCommandTestBuilder WithOwner(string owner)
        {
            _owner = owner;
            return this;
        }

        public ChargeCommandTestBuilder WithVatPayer(string vatPayer)
        {
            _vatPayer = vatPayer;
            return this;
        }

        public ChargeCommandTestBuilder WithStatus(int status)
        {
            _status = status;
            return this;
        }

        public ChargeCommand Build()
        {
            return new()
            {
                Document = new DocumentDto
                {
                    Id = "id",
                    Type = DocumentType.RequestUpdateChargeInformation,
                    RequestDate = SystemClock.Instance.GetCurrentInstant(),
                    IndustryClassification = IndustryClassification.Electricity,
                    CreatedDateTime = SystemClock.Instance.GetCurrentInstant(),
                    Recipient = new MarketParticipant
                    {
                        Id = "0",
                        BusinessProcessRole = MarketParticipantRole.EnergySupplier,
                    },
                    Sender = new MarketParticipant
                    {
                        Id = "1",
                        BusinessProcessRole = MarketParticipantRole.EnergySupplier,
                    },
                    BusinessReasonCode = BusinessReasonCode.UpdateChargeInformation,
                },
                ChargeOperation = new ChargeOperation
                {
                  Id = "id",
                  ChargeName = "description",
                  ChargeId = _mrid,
                  ChargeOwner = _owner,
                  StartDateTime = _validityStartDate,
                  Points = new List<Point>
                  {
                      new Point { Position = 0, Time = SystemClock.Instance.GetCurrentInstant(), Price = 200m },
                  },
                  Resolution = Resolution.PT1H,
                  Type = ChargeType.Fee,
                  VatClassification = VatClassification.Vat25,
                  ChargeDescription = "LongDescription",
                  TaxIndicator = _taxIndicator,
                },
            };
        }
    }
}
