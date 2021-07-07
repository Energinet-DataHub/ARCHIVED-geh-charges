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

using GreenEnergyHub.Charges.Domain.MarketDocument;
using GreenEnergyHub.Charges.Infrastructure.Messaging.Serialization.MarketDocument;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Messaging.Serialization.MarketDocument
{
    [UnitTest]
    public class DocumentTypeMapperTests
    {
        [Theory]
        [InlineData("D05", DocumentType.RequestChangeBillingMasterData)]
        [InlineData("D10", DocumentType.RequestUpdateChargeInformation)]
        [InlineData("", DocumentType.Unknown)]
        [InlineData("DoesNotExist", DocumentType.Unknown)]
        [InlineData(null, DocumentType.Unknown)]
        public void Map_WhenGivenInput_MapsToCorrectEnum(string unit, DocumentType expected)
        {
            var actual = DocumentTypeMapper.Map(unit);
            Assert.Equal(actual, expected);
        }
    }
}
