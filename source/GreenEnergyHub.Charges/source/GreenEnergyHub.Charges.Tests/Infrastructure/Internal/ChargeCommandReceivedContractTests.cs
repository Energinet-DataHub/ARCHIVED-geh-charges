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
using GreenEnergyHub.Charges.Domain.MarketDocument;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeCommandReceived;
using GreenEnergyHub.Charges.TestCore;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Internal
{
    [UnitTest]
    public class ChargeCommandReceivedContractTests
    {
        [Fact]
        public void ResolutionContract_ShouldBeSubsetOfResolution()
        {
            ProtoBufAssert.ContractEnumIsSubSet<ResolutionContract, Resolution>();
        }

        [Fact]
        public void VatClassificationContract_ShouldBeSubsetOfVatClassification()
        {
            ProtoBufAssert.ContractEnumIsSubSet<VatClassificationContract, VatClassification>();
        }

        [Fact]
        public void OperationTypeContract_ShouldBeSubsetOfOperationType()
        {
            ProtoBufAssert.ContractEnumIsSubSet<OperationTypeContract, OperationType>();
        }

        [Fact]
        public void DocumentTypeContract_ShouldBeSubsetOfDocumentType()
        {
            ProtoBufAssert.ContractEnumIsSubSet<DocumentTypeContract, DocumentType>();
        }

        [Fact]
        public void MarketParticipantRoleContract_ShouldBeSubsetOfMarketParticipantRole()
        {
            ProtoBufAssert.ContractEnumIsSubSet<MarketParticipantRoleContract, MarketParticipantRole>();
        }

        [Fact]
        public void IndustryClassificationContract_ShouldBeSubsetOfIndustryClassification()
        {
            ProtoBufAssert.ContractEnumIsSubSet<IndustryClassificationContract, IndustryClassification>();
        }

        [Fact]
        public void BusinessReasonCodeContract_ShouldBeSubsetOfBusinessReasonCode()
        {
            ProtoBufAssert.ContractEnumIsSubSet<BusinessReasonCodeContract, BusinessReasonCode>();
        }

        [Fact]
        public void ChargeTypeContract_ShouldBeSubsetOfChargeType()
        {
            ProtoBufAssert.ContractEnumIsSubSet<ChargeTypeContract, ChargeType>();
        }
    }
}
