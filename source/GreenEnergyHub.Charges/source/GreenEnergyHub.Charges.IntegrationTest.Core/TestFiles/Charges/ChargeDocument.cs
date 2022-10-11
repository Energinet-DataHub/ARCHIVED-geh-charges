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

namespace GreenEnergyHub.Charges.IntegrationTest.Core.TestFiles.Charges
{
    public static class ChargeDocument
    {
        public const string AnyValid = "TestFiles/Charges/TaxTariffWithPriceDocument.xml";
        public const string AnyWithPrice = "TestFiles/Charges/TaxTariffWithPriceDocument.xml";
        public const string CreateSubscription = "TestFiles/Charges/CreateSubscriptionWithPriceDocument.xml";
        public const string CreateTariff = "TestFiles/Charges/CreateTariffDocument.xml";
        public const string UpdateTariff = "TestFiles/Charges/UpdateTariffDocument.xml";
        public const string TaxTariffWithPrice = "TestFiles/Charges/TaxTariffWithPriceDocument.xml";
        public const string ChargeDocumentWhereSenderIdDoNotMatchAuthorizedActorId = "TestFiles/Charges/ChargeDocumentWhereSenderIdDoNotMatchAuthorizedActorId.xml";
        public const string AnyInvalid = "TestFiles/Charges/InvalidTariffDocument.xml";
        public const string TariffInvalidSchema = "TestFiles/Charges/InvalidSchemaTariffDocument.xml";
        public const string TariffBundleWithValidAndInvalid = "TestFiles/Charges/TariffBundleWithValidAndInvalid.xml";
        public const string CreateTariffAsSystemOperator = "TestFiles/Charges/CreateTariffAsSystemOperator.xml";
        public const string BundleWithMultipleOperationsForSameTariff = "TestFiles/Charges/BundleWithMultipleOperationsForSameTariff.xml";
        public const string BundleWithTwoOperationsForSameTariffSecondOpViolatingVr903
            = "TestFiles/Charges/BundleWithTwoOperationsForSameTariffSecondOpViolatingVr903.xml";

        public const string CreateTaxTariffAsSystemOperator = "TestFiles/Charges/CreateTaxTariffAsSystemOperator.xml";

        public const string ChargeInformationTariffHourlySample = "TestFiles/Samples/Charges/ChargeInformationTariffHourlySample.xml";
        public const string ChargeInformationFeeMonthlySample = "TestFiles/Samples/Charges/ChargeInformationFeeMonthlySample.xml";
        public const string ChargeInformationSubscriptionMonthlySample = "TestFiles/Samples/Charges/ChargeInformationSubscriptionMonthlySample.xml";
        public const string BundledChargeInformationSample = "TestFiles/Samples/Charges/BundledChargeInformationSample.xml";
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
        public const string PriceSeriesTariffFromSystemOperator = "TestFiles/Charges/PriceSeries/PriceSeriesTariffFromSystemOperator.xml";
        public const string BundledTariffPriceSeriesFirstOperationInvalidMaximumPrice = "TestFiles/Charges/PriceSeries/BundledTariffPriceSeriesFirstOperationInvalidMaximumPrice.xml";
        public const string BundledSubscriptionPriceSeriesSecondOperationChargeOwnerMismatch = "TestFiles/Charges/PriceSeries/BundledSubscriptionPriceSeriesSecondOperationChargeOwnerMismatch.xml";
        public const string BundledSubscriptionPriceSeries = "TestFiles/Charges/PriceSeries/BundledSubscriptionPriceSeries.xml";
    }
}
