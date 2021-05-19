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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using GreenEnergyHub.Charges.Application.Validation.InputValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using Xunit;
using Xunit.Categories;
using ChargeType = GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction.ChargeType;

namespace GreenEnergyHub.Charges.Tests.Application.Validation.InputValidation.ValidationRules
{
    [UnitTest]
    public class ChargePriceCountRuleTests
    {
        [Theory]
        [InlineAutoMoqData(1, false)]
        [InlineAutoMoqData(23, false)]
        [InlineAutoMoqData(24, true)]
        [InlineAutoMoqData(25, false)]
        [InlineAutoMoqData(96, false)]
        public void ChargePriceCountRule_WhenCalledWithPT1H_Expects24PricePoints(
            int priceCount,
            bool expected,
            [NotNull]ChargeCommand chargeCommand,
            Point point)
        {
            // Arrange
            chargeCommand.ChargeOperation.Type = ChargeType.Tariff;
            chargeCommand.ChargeOperation.Resolution = Resolution.PT1H;
            chargeCommand.ChargeOperation.Points = new List<Point>(GeneratePricePointList(point, priceCount));

            // Act
            var sut = new ChargePriceCountRule(chargeCommand);

            // Assert
            sut.IsValid.Should().Be(expected);
        }

        [Theory]
        [InlineAutoMoqData(0, false)]
        [InlineAutoMoqData(1, true)]
        [InlineAutoMoqData(2, false)]
        [InlineAutoMoqData(24, false)]
        [InlineAutoMoqData(96, false)]
        public void ChargePriceCountRule_WhenCalledWithP1D_Expects1PricePoint(
            int priceCount,
            bool expected,
            [NotNull]ChargeCommand chargeCommand,
            Point point)
        {
            // Arrange
            chargeCommand.ChargeOperation.Type = ChargeType.Tariff;
            chargeCommand.ChargeOperation.Resolution = Resolution.P1D;
            chargeCommand.ChargeOperation.Points = new List<Point>(GeneratePricePointList(point, priceCount));

            // Act
            var sut = new ChargePriceCountRule(chargeCommand);

            // Assert
            sut.IsValid.Should().Be(expected);
        }

        [Theory]
        [InlineAutoMoqData(0, false)]
        [InlineAutoMoqData(1, false)]
        [InlineAutoMoqData(2, false)]
        [InlineAutoMoqData(24, false)]
        [InlineAutoMoqData(95, false)]
        [InlineAutoMoqData(96, true)]
        [InlineAutoMoqData(97, false)]
        public void ChargePriceCountRule_WhenCalledWithPt15M_Expects96PricePoint(
            int priceCount,
            bool expected,
            [NotNull]ChargeCommand chargeCommand,
            Point point)
        {
            // Arrange
            chargeCommand.ChargeOperation.Type = ChargeType.Tariff;
            chargeCommand.ChargeOperation.Resolution = Resolution.PT15M;
            chargeCommand.ChargeOperation.Points = new List<Point>(GeneratePricePointList(point, priceCount));

            // Act
            var sut = new ChargePriceCountRule(chargeCommand);

            // Assert
            sut.IsValid.Should().Be(expected);
        }

        [Theory]
        [InlineAutoMoqData(ChargeType.Fee)]
        [InlineAutoMoqData(ChargeType.Subscription)]
        [InlineAutoMoqData(ChargeType.Unknown)]
        public void ChargePriceCountRule_WhenNotTariff_ShouldParseValidation(
            ChargeType chargeType,
            [NotNull] ChargeCommand chargeCommand)
        {
            chargeCommand.ChargeOperation.Type = chargeType;
            var sut = new ChargePriceCountRule(chargeCommand);
            sut.IsValid.Should().BeTrue();
        }

        private static List<Point> GeneratePricePointList(Point point, int itemsInList)
        {
            var pointList = new List<Point>();
            for (var i = 0; i < itemsInList; i++)
            {
                pointList.Add(point);
            }

            return pointList;
        }
    }
}
