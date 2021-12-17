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
using GreenEnergyHub.Charges.MessageHub.Application.Charges;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeReceiptData;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.TestFiles;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.Charges.MessageHub
{
    [UnitTest]
    public class ChargeConfirmationSpecificationTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void GetMessageWeight_WhenCalled_ReturnsConstant(
            AvailableChargeReceiptData availableData,
            ChargeConfirmationBundleSpecification sut)
        {
            var actual = sut.GetMessageWeight(availableData);

            actual.Should().Be(2);
        }

        [Fact]
        public void SizeOfMaximumDocument_ShouldNotExceedDefinedWeight()
        {
            // Arrange
            var confirmationMessageWeightInBytes = (long)ChargeConfirmationBundleSpecification.MessageWeight * 1000;

            // Act
            var xmlSizeInBytes = new System.IO.FileInfo(FilesForCalculatingBundleSize.WorstCaseChargeReceipt).Length;

            // Assert
            xmlSizeInBytes.Should().BeLessOrEqualTo(confirmationMessageWeightInBytes);
        }
    }
}
