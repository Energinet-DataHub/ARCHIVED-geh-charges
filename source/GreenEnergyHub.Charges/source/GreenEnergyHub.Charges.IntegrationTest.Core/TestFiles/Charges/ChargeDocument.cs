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
        public const string CreateSubscription = "TestFiles/Charges/CreateSubscriptionDocument.xml";
        public const string TaxTariffWithPrice = "TestFiles/Charges/TaxTariffWithPriceDocument.xml";
        public const string AnyInvalid = "TestFiles/Charges/InvalidTariffDocument.xml";
        public const string TariffInvalidSchema = "TestFiles/Charges/InvalidSchemaTariffDocument.xml";
        public const string TariffBundleWithValidAndInvalid = "TestFiles/Charges/TariffBundleWithValidAndInvalid.xml";
        public const string TariffBundleWithCreateAndUpdate = "TestFiles/Charges/TariffBundleWithCreateAndUpdate.xml";
        public const string BundleWithMultipleOperationsForSameTaxTariff = "TestFiles/Charges/BundleWithMultipleOperationsForSameTaxTariff.xml";
        public const string BundleWithTwoOperationsForSameTariffSecondOpViolatingVr903
            = "TestFiles/Charges/BundleWithTwoOperationsForSameTariffSecondOpViolatingVr903.xml";

        public const string TariffHourlyPricesSample = "TestFiles/Charges/Samples/TariffHourlyPricesSample.xml";
        public const string FeeMonthlyPriceSample = "TestFiles/Charges/Samples/FeeMonthlyPriceSample.xml";
        public const string SubscriptionMonthlyPriceSample = "TestFiles/Charges/Samples/SubscriptionMonthlyPriceSample.xml";
    }
}
