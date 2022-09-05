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

namespace GreenEnergyHub.Charges.IntegrationTest.Core.TestFiles.ChargeLinks
{
    public static class ChargeLinkDocument
    {
        public const string AnyValid = "TestFiles/ChargeLinks/FixedPeriodTaxTariffChargeLinkDocument.xml";
        public const string ChargeLinkDocumentWhereSenderIdDoNotMatchAuthorizedActorId = "TestFiles/ChargeLinks/ChargeLinkDocumentWhereSenderIdDoNotMatchAuthorizedActorId.xml";
        public const string TaxWithCreateAndUpdateDueToOverLappingPeriod = "TestFiles/ChargeLinks/TaxTariffWithCreateAndUpdateDueToOverLappingPeriodChargeLinkDocument.xml";
        public const string InvalidSchema = "TestFiles/ChargeLinks/InvalidSchemaChargeLinkDocument.xml";
        public const string ChargeLinkSubscriptionSample = "TestFiles/ChargeLinks/Samples/ChargeLinkSubscriptionSample.xml";
    }
}
