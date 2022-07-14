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
using GreenEnergyHub.Charges.Application.Charges.Acknowledgement;
using GreenEnergyHub.Charges.Application.Charges.Handlers;
using GreenEnergyHub.Charges.Application.Persistence;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
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
            [Frozen] Mock<IInputValidator<ChargeOperationDto>> inputValidator,
            [Frozen] Mock<IBusinessValidator<ChargeOperationDto>> businessValidator,
            [Frozen] Mock<IChargeCommandReceiptService> receiptService,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            [Frozen] Mock<IUnitOfWork> unitOfWork,
            TestMarketParticipant sender,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            ChargeBuilder chargeBuilder,
            ChargeCommandReceivedEvent receivedEvent,
            ChargePriceEventHandlerDeprecated sut)
        {
            // Arrange
            var validationResult = ValidationResult.CreateSuccess();
            SetupValidators(inputValidator, businessValidator, validationResult);
            var points = new List<Point>
            {
                new(0, 1.00m, InstantHelper.GetTodayPlusDaysAtMidnightUtc(0)),
                new(0, 2.00m, InstantHelper.GetTodayPlusDaysAtMidnightUtc(1)),
            };
            var charge = chargeBuilder.WithPoints(points).Build();
            SetupChargeRepository(chargeRepository, charge);
            SetupMarketParticipantRepository(marketParticipantRepository, sender);
            SetupChargeIdentifierFactoryMock(chargeIdentifierFactory);

            var confirmed = false;
            receiptService
                .Setup(s => s.AcceptValidOperationsAsync(It.IsAny<IReadOnlyCollection<ChargeOperationDto>>(), It.IsAny<DocumentDto>()))
                .Callback<IReadOnlyCollection<ChargeOperationDto>, DocumentDto>((_, _) => confirmed = true);

            // Act
            await sut.HandleAsync(receivedEvent);

            // Assert
            unitOfWork.Verify(x => x.SaveChangesAsync(), Times.AtLeastOnce());
            confirmed.Should().Be(true);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_WhenValidationFails_RejectsEvent(
            [Frozen] Mock<IChargeIdentifierFactory> chargeIdentifierFactory,
            [Frozen] Mock<IInputValidator<ChargeOperationDto>> inputValidator,
            [Frozen] Mock<IBusinessValidator<ChargeOperationDto>> businessValidator,
            [Frozen] Mock<IChargeCommandReceiptService> receiptService,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            TestMarketParticipant sender,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            ChargeBuilder chargeBuilder,
            ChargeCommandReceivedEvent receivedEvent,
            ChargePriceEventHandlerDeprecated sut)
        {
            // Arrange
            var validationResult = GetFailedValidationResult();
            SetupValidators(inputValidator, businessValidator, validationResult);
            var rejected = false;
            var charge = chargeBuilder.Build();
            SetupChargeRepository(chargeRepository, charge);
            SetupMarketParticipantRepository(marketParticipantRepository, sender);
            SetupChargeIdentifierFactoryMock(chargeIdentifierFactory);

            receiptService
                .Setup(s => s.RejectInvalidOperationsAsync(
                        It.IsAny<IReadOnlyCollection<ChargeOperationDto>>(),
                        It.IsAny<DocumentDto>(),
                        It.IsAny<IList<IValidationRuleContainer>>()))
                .Callback<IReadOnlyCollection<ChargeOperationDto>, DocumentDto, IList<IValidationRuleContainer>>((_, _, _) => rejected = true);

            // Act
            await sut.HandleAsync(receivedEvent);

            // Assert
            rejected.Should().Be(true);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_IfEventIsNull_ThrowsArgumentNullException(
            ChargePriceEventHandlerDeprecated sut)
        {
            // Arrange
            ChargeCommandReceivedEvent? receivedEvent = null;

            // Act / Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.HandleAsync(receivedEvent!));
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_WhenPriceSeriesWithResolutionPT1H_StorePriceSeries(
            [Frozen] Mock<IChargeIdentifierFactory> chargeIdentifierFactory,
            [Frozen] Mock<IInputValidator<ChargeOperationDto>> inputValidator,
            [Frozen] Mock<IBusinessValidator<ChargeOperationDto>> businessValidator,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            TestMarketParticipant sender,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            ChargeBuilder chargeBuilder,
            ChargePriceEventHandlerDeprecated sut)
        {
            // Arrange
            var validationResult = ValidationResult.CreateSuccess();
            SetupValidators(inputValidator, businessValidator, validationResult);
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
            var receivedEvent = new ChargeCommandReceivedEvent(InstantHelper.GetTodayAtMidnightUtc(), chargeCommand);

            // Act
            await sut.HandleAsync(receivedEvent);

            // Assert
            charge.Points.Count.Should().Be(24);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_WhenChargeUpdateHasStartDateAfterStopDate_RejectCurrentAndAllSubsequentOperations(
            [Frozen] Mock<IChargeIdentifierFactory> chargeIdentifierFactory,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            [Frozen] Mock<IDocumentValidator<ChargeInformationCommand>> documentValidator,
            [Frozen] Mock<IInputValidator<ChargeOperationDto>> inputValidator,
            [Frozen] Mock<IChargeCommandReceiptService> receiptService,
            TestMarketParticipant sender,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            ChargeBuilder chargeBuilder,
            ChargePriceEventHandlerDeprecated sut)
        {
            // Arrange
            var charge = chargeBuilder.WithStopDate(InstantHelper.GetTodayAtMidnightUtc()).Build();
            var receivedEvent = CreateInvalidOperationBundle();
            chargeRepository
                .Setup(r => r.SingleOrNullAsync(It.IsAny<ChargeIdentifier>()))!
                .ReturnsAsync(charge);
            SetupMarketParticipantRepository(marketParticipantRepository, sender);
            SetupChargeIdentifierFactoryMock(chargeIdentifierFactory);
            SetupValidatorsForOperation(documentValidator, inputValidator);

            var accepted = 0;
            receiptService
                .Setup(s => s.AcceptValidOperationsAsync(
                    It.IsAny<IReadOnlyCollection<ChargeOperationDto>>(),
                    It.IsAny<DocumentDto>()))
                .Callback<IReadOnlyCollection<ChargeOperationDto>, DocumentDto>((_, _) => accepted++);
            var rejectedRules = new List<IValidationRuleContainer>();
            receiptService
                .Setup(s => s.RejectInvalidOperationsAsync(
                    It.IsAny<IReadOnlyCollection<ChargeOperationDto>>(),
                    It.IsAny<DocumentDto>(),
                    It.IsAny<IList<IValidationRuleContainer>>()))
                .Callback<IReadOnlyCollection<ChargeOperationDto>, DocumentDto, IList<IValidationRuleContainer>>(
                    (_, _, s) => rejectedRules.AddRange(s));

            // Act
            await sut.HandleAsync(receivedEvent);

            // Assert
            accepted.Should().Be(1);
            var invalid = rejectedRules.Where(vr =>
                vr.ValidationRule.ValidationRuleIdentifier == ValidationRuleIdentifier.UpdateChargeMustHaveEffectiveDateBeforeOrOnStopDate);
            var subsequent = rejectedRules.Where(vr =>
                vr.ValidationRule.ValidationRuleIdentifier == ValidationRuleIdentifier.SubsequentBundleOperationsFail);
            var other = rejectedRules.Where(vr =>
                vr.ValidationRule.ValidationRuleIdentifier != ValidationRuleIdentifier.UpdateChargeMustHaveEffectiveDateBeforeOrOnStopDate &&
                vr.ValidationRule.ValidationRuleIdentifier != ValidationRuleIdentifier.SubsequentBundleOperationsFail);

            rejectedRules.Count.Should().Be(3);
            invalid.Count().Should().Be(1);
            subsequent.Count().Should().Be(2);
            other.Count().Should().Be(0);
        }

        private static ChargeCommandReceivedEvent CreateInvalidOperationBundle()
        {
            var validChargeOperationDto = new ChargeOperationDtoBuilder()
                .WithDescription("valid")
                .WithPointsInterval(
                    InstantHelper.GetYesterdayAtMidnightUtc(),
                    InstantHelper.GetTodayAtMidnightUtc())
                .WithPointWithXNumberOfPrices(24)
                .Build();
            var invalidChargeOperationDto = new ChargeOperationDtoBuilder()
                .WithDescription("invalid")
                .WithPointsInterval(
                    InstantHelper.GetTodayPlusDaysAtMidnightUtc(1),
                    InstantHelper.GetTodayPlusDaysAtMidnightUtc(2))
                .WithPointWithXNumberOfPrices(24)
                .Build();
            var failedChargeOperationDto = new ChargeOperationDtoBuilder()
                .WithDescription("failed")
                .WithPointsInterval(
                    InstantHelper.GetYesterdayAtMidnightUtc(),
                    InstantHelper.GetTodayAtMidnightUtc())
                .WithPointWithXNumberOfPrices(24)
                .Build();
            var chargeCommand = new ChargeInformationCommandBuilder()
                .WithChargeOperations(
                    new List<ChargeOperationDto>
                    {
                        validChargeOperationDto,
                        invalidChargeOperationDto,
                        failedChargeOperationDto,
                        failedChargeOperationDto,
                    })
                .Build();
            var receivedEvent = new ChargeCommandReceivedEvent(
                InstantHelper.GetTodayAtMidnightUtc(),
                chargeCommand);
            return receivedEvent;
        }

        private static void SetupValidatorsForOperation(
            Mock<IDocumentValidator<ChargeInformationCommand>> documentValidator,
            Mock<IInputValidator<ChargeOperationDto>> inputValidator)
        {
            inputValidator.Setup(v =>
                v.Validate(It.IsAny<ChargeOperationDto>())).Returns(ValidationResult.CreateSuccess());
            documentValidator.Setup(v =>
                v.ValidateAsync(It.IsAny<ChargeInformationCommand>())).ReturnsAsync(ValidationResult.CreateSuccess());
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

        private static void SetupValidators(
            Mock<IInputValidator<ChargeOperationDto>> inputValidator,
            Mock<IBusinessValidator<ChargeOperationDto>> businessValidator,
            ValidationResult validationResult)
        {
            inputValidator.Setup(v => v.Validate(It.IsAny<ChargeOperationDto>())).Returns(validationResult);
            businessValidator.Setup(v => v.ValidateAsync(It.IsAny<ChargeOperationDto>()))
                .Returns(Task.FromResult(validationResult));
        }

        private static ValidationResult GetFailedValidationResult()
        {
            var failedRule = new Mock<IValidationRule>();
            failedRule.Setup(r => r.IsValid).Returns(false);

            return ValidationResult.CreateFailure(
                new List<IValidationRuleContainer> { new DocumentValidationRuleContainer(failedRule.Object) });
        }

        private static ChargeInformationCommand CreateChargeCommandWith24Points()
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

            var operation = new ChargeOperationDtoBuilder()
                .WithPointsInterval(startDate, endDate)
                .WithPoints(points)
                .Build();

            return new ChargeInformationCommandBuilder()
                .WithChargeOperation(operation)
                .Build();
        }
    }
}
