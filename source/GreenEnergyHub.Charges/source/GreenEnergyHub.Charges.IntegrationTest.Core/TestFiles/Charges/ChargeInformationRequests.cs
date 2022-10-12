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

namespace GreenEnergyHub.Charges.IntegrationTest.Core.TestFiles.Charges
{
    public static class ChargeInformationRequests
    {
        public const string Subscription = "TestFiles/Charges/Subscription.xml";
        public const string Tariff = "TestFiles/Charges/Tariff.xml";
        public const string TariffAsSystemOperator = "TestFiles/Charges/TariffAsSystemOperator.xml";
        public const string TaxTariffAsSystemOperator = "TestFiles/Charges/TaxTariffAsSystemOperator.xml";
        public const string SenderIdDoNotMatchAuthorizedActorId = "TestFiles/Charges/SenderIdDoNotMatchAuthorizedActorId.xml";
        public const string InvalidTaxTariffAsGridAccessProvider = "TestFiles/Charges/InvalidTaxTariffAsGridAccessProvider.xml";

        public const string BundleWithMultipleOperationsForSameTariff = "TestFiles/Charges/BundleWithMultipleOperationsForSameTariff.xml";
        public const string BundleWithTwoOperationsForSameTariffSecondOpViolatingVr903 = "TestFiles/Charges/BundleWithTwoOperationsForSameTariffSecondOpViolatingVr903.xml";

        public const string ChargeInformationTariffHourlySample = "TestFiles/Samples/Charges/ChargeInformationTariffHourlySample.xml";
        public const string ChargeInformationFeeMonthlySample = "TestFiles/Samples/Charges/ChargeInformationFeeMonthlySample.xml";
        public const string ChargeInformationSubscriptionMonthlySample = "TestFiles/Samples/Charges/ChargeInformationSubscriptionMonthlySample.xml";
        public const string BundledChargeInformationSample = "TestFiles/Samples/Charges/BundledChargeInformationSample.xml";

        // Charge prices below - pending to be moved to new ChargePricesRequest and general clean-up
        public const string TariffBundleWithValidAndInvalid = "TestFiles/Charges/PriceSeries/TariffBundleWithValidAndInvalid.xml";
        public const string ChargePriceSeriesSubscriptionMonthlySample = "TestFiles/Samples/Charges/ChargePriceSeriesSubscriptionMonthlySample.xml";
        public const string ChargePriceSeriesFeeMonthlySample = "TestFiles/Samples/Charges/ChargePriceSeriesFeeMonthlySample.xml";
        public const string ChargePriceSeriesTariffHourlySample = "TestFiles/Samples/Charges/ChargePriceSeriesTariffHourlySample.xml";
        public const string BundledChargePriceSeriesSample = "TestFiles/Samples/Charges/BundledChargePriceSeriesSample.xml";
        public const string TariffPriceSeries = "TestFiles/Charges/PriceSeries/TariffPriceSeries.xml";
        public const string TaxTariffPriceSeriesWithInformationToBeIgnored = "TestFiles/Charges/PriceSeries/TaxTariffPriceSeriesWithInformationToBeIgnored.xml";
        public const string TariffPriceSeriesWithInvalidRecipientType = "TestFiles/Charges/PriceSeries/TariffPriceSeriesWithInvalidRecipientType.xml";
        public const string TariffPriceSeriesWithInvalidBusinessReasonCode = "TestFiles/Charges/PriceSeries/TariffPriceSeriesWithInvalidBusinessReasonCode.xml";
        public const string TariffPriceSeriesInvalidMaximumPrice = "TestFiles/Charges/PriceSeries/TariffPriceSeriesInvalidMaximumPrice.xml";
        public const string TariffPriceSeriesInvalidNumberOfPoints = "TestFiles/Charges/PriceSeries/TariffPriceSeriesInvalidNumberOfPointsMatchTimeIntervalAndResolution.xml";
        public const string TariffPriceSeriesInvalidPointsStart = "TestFiles/Charges/PriceSeries/TariffPriceSeriesInvalidPointsStart.xml";
        public const string TariffPriceSeriesInvalidStartAndEndDate = "TestFiles/Charges/PriceSeries/TariffPriceSeriesInvalidStartAndEndDate.xml";
        public const string PriceSeriesExistingFee = "TestFiles/Charges/PriceSeries/PriceSeriesExistingFee.xml";
        public const string PriceSeriesExistingTariff = "TestFiles/Charges/PriceSeries/PriceSeriesExistingTariff.xml";
        public const string PriceSeriesExistingSubscription = "TestFiles/Charges/PriceSeries/PriceSeriesExistingSubscription.xml";
        public const string PriceSeriesTaxTariff = "TestFiles/Charges/PriceSeries/PriceSeriesTaxTariff.xml";
        public const string BundledTariffPriceSeriesFirstOperationInvalidMaximumPrice = "TestFiles/Charges/PriceSeries/BundledTariffPriceSeriesFirstOperationInvalidMaximumPrice.xml";
        public const string BundledSubscriptionPriceSeriesSecondOperationChargeOwnerMismatch = "TestFiles/Charges/PriceSeries/BundledSubscriptionPriceSeriesSecondOperationChargeOwnerMismatch.xml";
        public const string BundledSubscriptionPriceSeries = "TestFiles/Charges/PriceSeries/BundledSubscriptionPriceSeries.xml";
    }
}
