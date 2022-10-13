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
    }
}
