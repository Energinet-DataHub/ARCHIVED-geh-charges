using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Message;
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
                    Status = _status,
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
