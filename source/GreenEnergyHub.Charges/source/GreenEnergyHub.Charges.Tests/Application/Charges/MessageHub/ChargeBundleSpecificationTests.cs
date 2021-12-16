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

using System;
using FluentAssertions;
using GreenEnergyHub.Charges.MessageHub.Application.Charges;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeData;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.TestFiles;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.Charges.MessageHub
{
    [UnitTest]
    public class ChargeBundleSpecificationTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void GetMessageWeight_WhenCalled_ReturnsExpectedWeight(
            AvailableChargeData availableData,
            ChargeBundleSpecification sut)
        {
            // Arrange
            var expected = (int)Math.Round(
                (availableData.Points.Count * ChargeBundleSpecification.ChargePointMessageWeight) + ChargeBundleSpecification.ChargeMessageWeight,
                MidpointRounding.AwayFromZero);

            // Act
            var actual = sut.GetMessageWeight(availableData);

            // Arrange
            actual.Should().Be(expected);
        }

        [Fact]
        public void SizeOfMaximumDocument_ShouldNotExceedDefinedWeight()
        {
            // Arrange
            var chargeMessageWeightInBytes = (long)ChargeBundleSpecification.ChargeMessageWeight * 1000;

            // Act
            var xmlSizeInBytes = new System.IO.FileInfo(BundleSize.WorstCaseChargeNoPoints).Length;

            // Assert
            xmlSizeInBytes.Should().BeLessOrEqualTo(chargeMessageWeightInBytes);
        }

        [Fact]
        public void SizeOfMaximumDocumentWith1000Points_ShouldNotExceedDefinedWeight()
        {
            // Arrange
            var numberOfPointsInXml = 1000;
            var convertMessageWeightToKb = 1000;
            var chargeMessageWeightInBytes =
                (long)(ChargeBundleSpecification.ChargeMessageWeight +
                       (ChargeBundleSpecification.ChargePointMessageWeight * numberOfPointsInXml))
                * convertMessageWeightToKb;

            // Act
            var xmlSizeInBytes = new System.IO.FileInfo(BundleSize.WorstCaseChargeWithPoints).Length;

            // Assert
            xmlSizeInBytes.Should().BeLessOrEqualTo(chargeMessageWeightInBytes);
        }
    }
}
