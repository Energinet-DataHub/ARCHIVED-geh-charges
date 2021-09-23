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
using GreenEnergyHub.Charges.Domain.ChargeCommands;
using GreenEnergyHub.Charges.Domain.ChargeCommands.Validation;
using GreenEnergyHub.Charges.Domain.ChargeCommands.Validation.InputValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.TestHelpers;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.Charges.Validation.InputValidation.ValidationRules
{
    [UnitTest]
    public class SubscriptionMustHaveSinglePriceRuleTests
    {
        [Theory]
        [InlineAutoMoqData(0, false)]
        [InlineAutoMoqData(1, true)]
        [InlineAutoMoqData(2, false)]
        public void IsValid_WhenCalledWith1PricePoint_ShouldParseValidation(
            int priceCount,
            bool expected,
            [NotNull]ChargeCommand chargeCommand,
            Point point)
        {
            // Arrange
            chargeCommand.ChargeOperation.Type = ChargeType.Subscription;
            chargeCommand.ChargeOperation.Points = new List<Point>(GeneratePricePointList(point, priceCount));

            // Act
            var sut = new SubscriptionMustHaveSinglePriceRule(chargeCommand);

            // Assert
            sut.IsValid.Should().Be(expected);
        }

        [Theory]
        [InlineAutoMoqData(ChargeType.Tariff)]
        [InlineAutoMoqData(ChargeType.Unknown)]
        public void IsValid_NeitherFeeOrSubscription_ShouldParseValidation(
            ChargeType chargeType,
            [NotNull] ChargeCommand chargeCommand)
        {
            chargeCommand.ChargeOperation.Type = chargeType;
            var sut = new SubscriptionMustHaveSinglePriceRule(chargeCommand);
            sut.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationRuleIdentifier_ShouldBe_EqualTo([NotNull] ChargeCommand command)
        {
            var sut = new SubscriptionMustHaveSinglePriceRule(command);
            sut.ValidationRuleIdentifier.Should().Be(ValidationRuleIdentifier.SubscriptionMustHaveSinglePrice);
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
