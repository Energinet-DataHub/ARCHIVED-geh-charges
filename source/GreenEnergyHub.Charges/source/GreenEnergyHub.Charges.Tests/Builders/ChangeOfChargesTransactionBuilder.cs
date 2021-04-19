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

using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
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

        public ChangeOfChargesTransaction Build()
        {
            var test = new ChangeOfChargesTransaction
            {
                ChargeTypeMRid = _mrid,
                MktActivityRecord = new MktActivityRecord
                {
                    Status = (MktActivityRecordStatus)_status,
                    ValidityStartDate = _validityStartDate,
                    ChargeType = new ChargeType
                    {
                      VatPayer = _vatPayer,
                      TaxIndicator = _taxIndicator,
                    },
                },
                ChargeTypeOwnerMRid = _owner,
            };

            return test;
        }
    }
}
