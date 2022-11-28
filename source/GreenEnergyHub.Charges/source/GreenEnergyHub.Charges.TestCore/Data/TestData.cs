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
using Energinet.DataHub.Charges.Contracts.Charge;

namespace GreenEnergyHub.Charges.TestCore.Data
{
    /// <summary>
    /// Provide a unified compile time safe way to access values corresponding to the seeded test data
    /// in the test database.
    /// </summary>
    public static class TestData
    {
        public static class Charge
        {
            public static class TestTar001
            {
                public const string SenderProvidedChargeId = "TestTar001";
                public const string Name = "Tariff with multiple periods";
                public const int NoOfPeriods = 4;
                public const string ChargeOwnerId = SeededData.MarketParticipants.Provider8100000000030.Gln;
            }
        }

        public static class ChargeHistory
        {
            public static class HistTar001
            {
                public const string SenderProvidedChargeId = "HistTar001";
                public const string Name = "HistTar001 Name";
                public const string Description = "HistTar001 Description";
                public const string ChargeOwner = SeededData.MarketParticipants.Provider8100000000030.Gln;
                public const Resolution Resolution = Energinet.DataHub.Charges.Contracts.Charge.Resolution.PT1H;
                public const VatClassification VatClassification = Energinet.DataHub.Charges.Contracts.Charge.VatClassification.NoVat;
                public const ChargeType ChargeType = Energinet.DataHub.Charges.Contracts.Charge.ChargeType.D03;
                public static readonly DateTimeOffset StartDateTime = new(2021, 12, 31, 23, 00, 00, TimeSpan.FromHours(0));
                public static readonly bool TaxIndicator = false;
                public static readonly bool TransparentInvoicing = false;
            }

            public static class TariffA
            {
                public const string SenderProvidedChargeId = "TariffA";
                public const ChargeType ChargeType = Energinet.DataHub.Charges.Contracts.Charge.ChargeType.D03;
                public const string ChargeOwner = SeededData.MarketParticipants.Provider8100000000030.Gln;
            }
        }
    }
}
