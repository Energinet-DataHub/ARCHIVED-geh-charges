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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using GreenEnergyHub.Charges.Application.Validation.InputValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using GreenEnergyHub.Charges.TestCore;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.Validation.InputValidation.ValidationRules
{
    [UnitTest]
    public class ChargePriceMaximumDigitsRuleTests
    {
        [Theory]
        [InlineAutoMoqData(0.000001, true)]
        [InlineAutoMoqData(99999999.000001, true)]
        [InlineAutoMoqData(99999999.0000001, false)]
        [InlineAutoMoqData(99999999, true)]
        [InlineAutoMoqData(100000000.000001, false)]
        public void ChargePriceMaximumDigitsAndDecimalsRule_WhenCalledWithValidPrice_ShouldReturnValid(
            decimal price,
            bool expected,
            [NotNull] ChargeCommand command,
            [NotNull] Point point)
        {
            // Arrange
            point.Price = price;
            command.ChargeOperation.Points = new List<Point> { point };

            // Act
            var sut = new ChargePriceMaximumDigitsAndDecimalsRule(command);

            // Assert
            sut.IsValid.Should().Be(expected);
        }
    }
}
