﻿// Copyright 2020 Energinet DataHub A/S
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
using GreenEnergyHub.Charges.MessageHub.BundleSpecification.Charges;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeData;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.TestFiles;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.MessageHub.BundleSpecification.Charges
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
            var expected = (int)Math.Round(ChargeBundleSpecification.ChargeMessageWeight, MidpointRounding.AwayFromZero);

            // Act
            var actual = sut.GetMessageWeight(availableData);

            // Arrange
            actual.Should().Be(expected);
        }

        [Fact]
        public void SizeOfMaximumDocument_ShouldNotExceedDefinedWeight()
        {
            // Arrange
            var convertMessageWeightToKb = 1000;
            var chargeMessageWeightInBytes =
                (long)ChargeBundleSpecification.ChargeMessageWeight * convertMessageWeightToKb;

            // Act
            var xmlSizeInBytes =
                new System.IO.FileInfo(FilesForCalculatingBundleSize.WorstCaseChargeNoPoints).Length;

            // Assert
            xmlSizeInBytes.Should().BeLessOrEqualTo(chargeMessageWeightInBytes);
        }
    }
}
