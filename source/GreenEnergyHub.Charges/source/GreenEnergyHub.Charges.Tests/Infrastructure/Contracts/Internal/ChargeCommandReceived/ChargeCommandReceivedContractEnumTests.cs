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

using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeCommandReceived;
using GreenEnergyHub.Charges.Tests.Protobuf;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Contracts.Internal.ChargeCommandReceived
{
    [UnitTest]
    public class ChargeCommandReceivedContractEnumTests
    {
        [Fact]
        public void ResolutionContract_ShouldBeSubsetOfResolution()
        {
            ProtobufAssert.ContractEnumIsSubSet<ResolutionContract, Resolution>();
        }

        [Fact]
        public void VatClassificationContract_ShouldBeSubsetOfVatClassification()
        {
            ProtobufAssert.ContractEnumIsSubSet<VatClassificationContract, VatClassification>();
        }

        [Fact]
        public void DocumentTypeContract_ShouldBeSubsetOfDocumentType()
        {
            ProtobufAssert.ContractEnumIsSubSet<DocumentTypeContract, DocumentType>();
        }

        [Fact]
        public void MarketParticipantRoleContract_ShouldBeSubsetOfMarketParticipantRole()
        {
            ProtobufAssert.ContractEnumIsSubSet<MarketParticipantRoleContract, MarketParticipantRole>();
        }

        [Fact]
        public void IndustryClassificationContract_ShouldBeSubsetOfIndustryClassification()
        {
            ProtobufAssert.ContractEnumIsSubSet<IndustryClassificationContract, IndustryClassification>();
        }

        [Fact]
        public void BusinessReasonCodeContract_ShouldBeSubsetOfBusinessReasonCode()
        {
            ProtobufAssert.ContractEnumIsSubSet<BusinessReasonCodeContract, BusinessReasonCode>();
        }

        [Fact]
        public void ChargeTypeContract_ShouldBeSubsetOfChargeType()
        {
            ProtobufAssert.ContractEnumIsSubSet<ChargeTypeContract, ChargeType>();
        }
    }
}
