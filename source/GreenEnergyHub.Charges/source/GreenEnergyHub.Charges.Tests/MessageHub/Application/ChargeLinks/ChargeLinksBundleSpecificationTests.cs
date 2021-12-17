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

using FluentAssertions;
using GreenEnergyHub.Charges.MessageHub.Application.ChargeLinks;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeLinksData;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.TestFiles;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.MessageHub.Application.ChargeLinks
{
    [UnitTest]
    public class ChargeLinksBundleSpecificationTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void GetMessageWeight_WhenCalled_ReturnsConstant(
            AvailableChargeLinksData availableData,
            ChargeLinksBundleSpecification sut)
        {
            var actual = sut.GetMessageWeight(availableData);

            actual.Should().Be(2);
        }

        [Fact]
        public void NotifyAsync_SizeOfMaximumDocument_ShouldNotExceedDefinedWeight()
        {
            // Arrange
            var confirmationMessageWeightInBytes = (long)ChargeLinksBundleSpecification.MessageWeight * 1000;

            // Act
            var xmlSizeInBytes = new System.IO.FileInfo(BundleSize.WorstCaseChargeLink).Length;

            // Assert
            xmlSizeInBytes.Should().BeLessOrEqualTo(confirmationMessageWeightInBytes);
        }
    }
}
