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

namespace GreenEnergyHub.Charges.IntegrationTest.Core.TestFiles.Charges.PriceSeries
{
    public static class ChargePricesRequests
    {
        public const string ChargePriceSeriesSubscriptionMonthlySample = "TestFiles/Samples/Charges/ChargePriceSeriesSubscriptionMonthlySample.xml";
        public const string ChargePriceSeriesFeeMonthlySample = "TestFiles/Samples/Charges/ChargePriceSeriesFeeMonthlySample.xml";
        public const string ChargePriceSeriesTariffHourlySample = "TestFiles/Samples/Charges/ChargePriceSeriesTariffHourlySample.xml";
        public const string BundledChargePriceSeriesSample = "TestFiles/Samples/Charges/BundledChargePriceSeriesSample.xml";

        public const string TariffPriceSeries = "TestFiles/Charges/PriceSeries/TariffPriceSeries.xml";
        public const string TariffPriceSeriesInvalidMaximumPrice = "TestFiles/Charges/PriceSeries/TariffPriceSeriesInvalidMaximumPrice.xml";
        public const string TariffPriceSeriesInvalidNumberOfPoints = "TestFiles/Charges/PriceSeries/TariffPriceSeriesInvalidNumberOfPointsMatchTimeIntervalAndResolution.xml";
        public const string TariffPriceSeriesInvalidPointsStart = "TestFiles/Charges/PriceSeries/TariffPriceSeriesInvalidPointsStart.xml";
        public const string TariffPriceSeriesWithInvalidRecipientType = "TestFiles/Charges/PriceSeries/TariffPriceSeriesWithInvalidRecipientType.xml";
        public const string TariffPriceSeriesWithInvalidBusinessReasonCode = "TestFiles/Charges/PriceSeries/TariffPriceSeriesWithInvalidBusinessReasonCode.xml";
        public const string TaxTariffPriceSeriesWithInformationToBeIgnored = "TestFiles/Charges/PriceSeries/TaxTariffPriceSeriesWithInformationToBeIgnored.xml";
        public const string TaxTariffPriceSeries = "TestFiles/Charges/PriceSeries/TaxTariffPriceSeries.xml";
        public const string IrregularPriceSeriesInvalidEndDate = "TestFiles/Charges/PriceSeries/IrregularPriceSeriesInvalidEndDate.xml";
        public const string UnsupportedResolution = "TestFiles/Charges/PriceSeries/UnsupportedResolution.xml";

        public const string BundledTariffPriceSeriesFirstOperationInvalidMaximumPrice = "TestFiles/Charges/PriceSeries/BundledTariffPriceSeriesFirstOperationInvalidMaximumPrice.xml";
        public const string BundledSubscriptionPriceSeriesSecondOperationChargeOwnerMismatch = "TestFiles/Charges/PriceSeries/BundledSubscriptionPriceSeriesSecondOperationChargeOwnerMismatch.xml";
        public const string BundledSubscriptionPriceSeries = "TestFiles/Charges/PriceSeries/BundledSubscriptionPriceSeries.xml";
    }
}
