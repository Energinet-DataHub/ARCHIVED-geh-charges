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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using FluentAssertions;
using GreenEnergyHub.Charges.Application.Charges.Handlers;
using GreenEnergyHub.Charges.Application.Charges.Services;
using GreenEnergyHub.Charges.Application.Persistence;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using GreenEnergyHub.Charges.Tests.Builders.Testables;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.Charges.Handlers
{
    [UnitTest]
    public class ChargePriceEventHandlerTests
    {
        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_WhenValidationSucceed_StoreAndConfirmCommand(
            [Frozen] Mock<IChargeIdentifierFactory> chargeIdentifierFactory,
            [Frozen] Mock<IInputValidator<ChargePriceOperationDto>> inputValidator,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            [Frozen] Mock<IUnitOfWork> unitOfWork,
            TestMarketParticipant sender,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<IChargePriceRejectionService> chargePriceRejectionService,
            [Frozen] Mock<IChargePriceConfirmationService> chargePriceConfirmationService,
            ChargeBuilder chargeBuilder,
            ChargePriceCommandReceivedEvent receivedEvent,
            ChargePriceEventHandler sut)
        {
            // Arrange
            var validationResult = ValidationResult.CreateSuccess();
            inputValidator
                .Setup(v => v.Validate(It.IsAny<ChargePriceOperationDto>())).Returns(validationResult);
            var points = new List<Point>
            {
                new(0, 1.00m, InstantHelper.GetTodayPlusDaysAtMidnightUtc(0)),
                new(0, 2.00m, InstantHelper.GetTodayPlusDaysAtMidnightUtc(1)),
            };
            var charge = chargeBuilder.WithPoints(points).Build();
            SetupChargeRepository(chargeRepository, charge);
            SetupMarketParticipantRepository(marketParticipantRepository, sender);
            SetupChargeIdentifierFactoryMock(chargeIdentifierFactory);

            // Act
            await sut.HandleAsync(receivedEvent);

            // Assert
            chargePriceRejectionService.Verify(x =>
                x.SaveRejectionsAsync(
                    It.Is<List<ChargePriceOperationDto>>(x => x.Count == 0),
                    It.IsAny<List<IValidationRuleContainer>>()));
            chargePriceConfirmationService.Verify(x =>
                x.SaveConfirmationsAsync(
                    It.Is<List<ChargePriceOperationDto>>(x => x.Count == 3)));
            unitOfWork.Verify(x => x.SaveChangesAsync(), Times.AtLeastOnce());
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_WhenValidationFails_RejectsEvent(
            [Frozen] Mock<IChargeIdentifierFactory> chargeIdentifierFactory,
            [Frozen] Mock<IInputValidator<ChargePriceOperationDto>> inputValidator,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            TestMarketParticipant sender,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<IChargePriceRejectionService> chargePriceRejectionService,
            ChargeBuilder chargeBuilder,
            ChargePriceCommandReceivedEvent receivedEvent,
            ChargePriceEventHandler sut)
        {
            // Arrange
            var validationResult = GetFailedValidationResult();
            inputValidator.Setup(v => v.Validate(It.IsAny<ChargePriceOperationDto>())).Returns(validationResult);
            var charge = chargeBuilder.Build();
            SetupChargeRepository(chargeRepository, charge);
            SetupMarketParticipantRepository(marketParticipantRepository, sender);
            SetupChargeIdentifierFactoryMock(chargeIdentifierFactory);

            // Act
            await sut.HandleAsync(receivedEvent);

            // Assert
            chargePriceRejectionService.Verify(x =>
                x.SaveRejectionsAsync(
                    It.Is<List<ChargePriceOperationDto>>(x => x.Count == 3),
                    It.IsAny<List<IValidationRuleContainer>>()));
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_IfEventIsNull_ThrowsArgumentNullException(
            ChargePriceEventHandler sut)
        {
            // Arrange
            ChargePriceCommandReceivedEvent? receivedEvent = null;

            // Act / Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.HandleAsync(receivedEvent!));
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_WhenPriceSeriesWithResolutionPT1H_StorePriceSeries(
            [Frozen] Mock<IChargeIdentifierFactory> chargeIdentifierFactory,
            [Frozen] Mock<IInputValidator<ChargePriceOperationDto>> inputValidator,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            TestMarketParticipant sender,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            ChargeBuilder chargeBuilder,
            ChargePriceEventHandler sut)
        {
            // Arrange
            var validationResult = ValidationResult.CreateSuccess();
            inputValidator.Setup(v => v.Validate(It.IsAny<ChargePriceOperationDto>())).Returns(validationResult);
            var points = new List<Point>();
            var price = 99.00M;
            for (var i = 0; i <= 23; i++)
            {
                points.Add(new Point(1 + i, price + i, InstantHelper.GetTodayAtMidnightUtc() + Duration.FromHours(i)));
            }

            var charge = chargeBuilder.WithPoints(points).Build();
            SetupChargeRepository(chargeRepository, charge);
            SetupMarketParticipantRepository(marketParticipantRepository, sender);
            SetupChargeIdentifierFactoryMock(chargeIdentifierFactory);

            var chargeCommand = CreateChargeCommandWith24Points();
            var receivedEvent = new ChargePriceCommandReceivedEvent(InstantHelper.GetTodayAtMidnightUtc(), chargeCommand);

            // Act
            await sut.HandleAsync(receivedEvent);

            // Assert
            charge.Points.Count.Should().Be(24);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_WhenChargeUpdateHasStartDateAfterStopDate_RejectCurrentAndAllSubsequentOperations(
            [Frozen] Mock<IChargeRepository> chargeRepository,
            [Frozen] Mock<IInputValidator<ChargePriceOperationDto>> inputValidator,
            TestMarketParticipant sender,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<IChargePriceConfirmationService> chargePriceConfirmationService,
            [Frozen] Mock<IChargePriceRejectionService> chargePriceRejectionService,
            ChargeBuilder chargeBuilder,
            ChargePriceEventHandler sut)
        {
            // Arrange
            var charge = chargeBuilder.WithStopDate(InstantHelper.GetTodayAtMidnightUtc()).Build();
            var receivedEvent = CreateInvalidOperationBundle();
            chargeRepository
                .Setup(r => r.SingleOrNullAsync(It.IsAny<ChargeIdentifier>()))!
                .ReturnsAsync(charge);
            SetupMarketParticipantRepository(marketParticipantRepository, sender);
            inputValidator.Setup(v =>
                v.Validate(It.IsAny<ChargePriceOperationDto>())).Returns(ValidationResult.CreateSuccess());
            var rejectedRules = new List<IValidationRuleContainer>();
            chargePriceRejectionService
                .Setup(s => s.SaveRejectionsAsync(
                    It.IsAny<List<ChargePriceOperationDto>>(),
                    It.IsAny<List<IValidationRuleContainer>>()))
                .Callback<IReadOnlyCollection<ChargePriceOperationDto>, IList<IValidationRuleContainer>>(
                    (_, s) => rejectedRules.AddRange(s));

            // Act
            await sut.HandleAsync(receivedEvent);

            // Assert
            chargePriceConfirmationService.Verify(x =>
                x.SaveConfirmationsAsync(
                    It.Is<List<ChargePriceOperationDto>>(x => x.Count == 1)));
            var invalid = rejectedRules.Where(vr =>
                vr.ValidationRule.ValidationRuleIdentifier == ValidationRuleIdentifier.UpdateChargeMustHaveEffectiveDateBeforeOrOnStopDate);
            var subsequent = rejectedRules.Where(vr =>
                vr.ValidationRule.ValidationRuleIdentifier == ValidationRuleIdentifier.SubsequentBundleOperationsFail);
            rejectedRules.Count.Should().Be(3);
            invalid.Count().Should().Be(1);
            subsequent.Count().Should().Be(2);
        }

        private static ChargePriceCommandReceivedEvent CreateInvalidOperationBundle()
        {
            var validChargeOperationDto = new ChargePriceOperationDtoBuilder()
                .WithPointsInterval(
                    InstantHelper.GetYesterdayAtMidnightUtc(),
                    InstantHelper.GetTodayAtMidnightUtc())
                .WithPointWithXNumberOfPrices(24)
                .Build();
            var invalidChargeOperationDto = new ChargePriceOperationDtoBuilder()
                .WithPointsInterval(
                    InstantHelper.GetTodayPlusDaysAtMidnightUtc(2),
                    InstantHelper.GetTodayPlusDaysAtMidnightUtc(1))
                .WithPointWithXNumberOfPrices(24)
                .Build();
            var failedChargeOperationDto = new ChargePriceOperationDtoBuilder()
                .WithPointsInterval(
                    InstantHelper.GetYesterdayAtMidnightUtc(),
                    InstantHelper.GetTodayAtMidnightUtc())
                .WithPointWithXNumberOfPrices(24)
                .Build();
            var chargeCommand = new ChargePriceCommandBuilder()
                .WithChargeOperations(
                    new List<ChargePriceOperationDto>
                    {
                        validChargeOperationDto,
                        invalidChargeOperationDto,
                        failedChargeOperationDto,
                        failedChargeOperationDto,
                    })
                .Build();
            var receivedEvent = new ChargePriceCommandReceivedEvent(
                InstantHelper.GetTodayAtMidnightUtc(),
                chargeCommand);
            return receivedEvent;
        }

        private static void SetupMarketParticipantRepository(
            Mock<IMarketParticipantRepository> marketParticipantRepository,
            TestMarketParticipant marketParticipant)
        {
            marketParticipantRepository
                .Setup(r => r.GetSystemOperatorOrGridAccessProviderAsync(It.IsAny<string>()))
                .ReturnsAsync(marketParticipant);
        }

        private static void SetupChargeIdentifierFactoryMock(Mock<IChargeIdentifierFactory> chargeIdentifierFactory)
        {
            chargeIdentifierFactory
                .Setup(x => x.CreateAsync(It.IsAny<string>(), It.IsAny<ChargeType>(), It.IsAny<string>()))
                .ReturnsAsync(It.IsAny<ChargeIdentifier>());
        }

        private static void SetupChargeRepository(
            [Frozen] Mock<IChargeRepository> chargeRepository,
            Charge charge)
        {
            chargeRepository
                .Setup(r => r.SingleOrNullAsync(It.IsAny<ChargeIdentifier>()))!
                .ReturnsAsync(charge);
        }

        private static ValidationResult GetFailedValidationResult()
        {
            var failedRule = new Mock<IValidationRule>();
            failedRule.Setup(r => r.IsValid).Returns(false);

            return ValidationResult.CreateFailure(
                new List<IValidationRuleContainer> { new DocumentValidationRuleContainer(failedRule.Object) });
        }

        private static ChargePriceCommand CreateChargeCommandWith24Points()
        {
            var points = new List<Point>();
            var price = 0.00M;
            var startTime = InstantHelper.GetTodayAtMidnightUtc();
            for (var i = 0; i <= 23; i++)
            {
                points.Add(new Point(1 + i, price + i, startTime + Duration.FromHours(i)));
            }

            var startDate = points.Min(x => x.Time);
            var endDate = points.Max(x => x.Time) + Duration.FromHours(1);

            var operation = new ChargePriceOperationDtoBuilder()
                .WithPointsInterval(startDate, endDate)
                .WithPoints(points)
                .Build();

            return new ChargePriceCommandBuilder()
                .WithChargeOperation(operation)
                .Build();
        }
    }
}
