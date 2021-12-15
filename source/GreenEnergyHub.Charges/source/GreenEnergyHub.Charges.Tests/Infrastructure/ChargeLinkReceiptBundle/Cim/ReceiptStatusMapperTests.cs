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
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.MarketDocument;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.ChargeLinkReceiptBundle.Cim
{
    [UnitTest]
    public class ReceiptStatusMapperTests
    {
        [Theory]
        [InlineData(ReceiptStatus.Confirmed, "A01")]
        [InlineData(ReceiptStatus.Rejected, "A02")]
        public void Map_WhenGivenKnownInput_MapsToCorrectString(ReceiptStatus receiptStatus, string expected)
        {
            var actual = ReceiptStatusMapper.Map(receiptStatus);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(ReceiptStatus.Unknown)]
        public void Map_WhenGivenUnknownInput_ThrowsExceptions(ReceiptStatus receiptStatus)
        {
            Assert.Throws<InvalidEnumArgumentException>(() => ReceiptStatusMapper.Map(receiptStatus));
        }
    }
}
