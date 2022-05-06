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

using System.ComponentModel;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.MarketDocument;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Core.Cim.MarketDocument
{
    [UnitTest]
    public class DocumentTypeMapperTests
    {
        [Theory]
        [InlineData("D05", DocumentType.RequestChangeBillingMasterData)]
        [InlineData("D06", DocumentType.ChargeLinkReceipt)]
        [InlineData("D07", DocumentType.NotifyBillingMasterData)]
        [InlineData("D10", DocumentType.RequestChangeOfPriceList)]
        [InlineData("D11", DocumentType.Unknown)]
        [InlineData("D12", DocumentType.NotifyPriceList)]
        [InlineData("", DocumentType.Unknown)]
        [InlineData("DoesNotExist", DocumentType.Unknown)]
        [InlineData(null, DocumentType.Unknown)]
        public void Map_WhenGivenInput_MapsToCorrectEnum(string unit, DocumentType expected)
        {
            var actual = DocumentTypeMapper.Map(unit);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(DocumentType.RequestChangeBillingMasterData, "D05")]
        [InlineData(DocumentType.ChargeLinkReceipt, "D06")]
        [InlineData(DocumentType.NotifyBillingMasterData, "D07")]
        [InlineData(DocumentType.RequestChangeOfPriceList, "D10")]
        [InlineData(DocumentType.ConfirmRequestChangeOfPriceList, "D11")]
        [InlineData(DocumentType.RejectRequestChangeOfPriceList, "D11")]
        [InlineData(DocumentType.NotifyPriceList, "D12")]
        public void Map_WhenGivenKnownInput_MapsToCorrectString(DocumentType documentType, string expected)
        {
            var actual = DocumentTypeMapper.Map(documentType);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(DocumentType.Unknown)]
        public void Map_WhenGivenUnknownInput_ThrowsExceptions(DocumentType documentType)
        {
            Assert.Throws<InvalidEnumArgumentException>(() => DocumentTypeMapper.Map(documentType));
        }
    }
}
