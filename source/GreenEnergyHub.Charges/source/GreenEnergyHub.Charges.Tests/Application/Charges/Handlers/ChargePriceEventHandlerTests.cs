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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using GreenEnergyHub.Charges.Tests.Builders.Testables;
using GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeCommands.Validation;
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
            [Frozen] Mock<IInputValidator<ChargePriceDto>> inputValidator,
            [Frozen] Mock<IBusinessValidator<ChargePriceDto>> businessValidator,
            [Frozen] Mock<IChargeCommandReceiptService> receiptService,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            [Frozen] Mock<IUnitOfWork> unitOfWork,
            TestMarketParticipant sender,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            ChargeBuilder chargeBuilder,
            ChargePriceDtoBuilder chargePriceDtoBuilder,
            ChargeCommandBuilder chargeCommandBuilder,
            ChargePriceEventHandler sut)
        {
            // Arrange
            var points = BuildPoints();
            var chargePriceDto = chargePriceDtoBuilder.WithPoints(points).Build();
            var chargeCommand = chargeCommandBuilder.WithChargeOperation(chargePriceDto).Build();
            var receivedEvent = new ChargeCommandReceivedEvent(Instant.MinValue, chargeCommand);
            var validationResult = ValidationResult.CreateSuccess();

            SetupValidators(inputValidator, businessValidator, validationResult);
            var charge = chargeBuilder.WithPoints(points).Build();
            SetupChargeRepository(chargeRepository, charge);
            SetupMarketParticipantRepository(marketParticipantRepository, sender);
            SetupChargeIdentifierFactoryMock(chargeIdentifierFactory);

            var confirmed = false;
            receiptService
                .Setup(s => s.AcceptValidOperationsAsync(It.IsAny<IReadOnlyCollection<IChargeOperation>>(), It.IsAny<DocumentDto>()))
                .Callback<IReadOnlyCollection<IChargeOperation>, DocumentDto>((_, _) => confirmed = true);

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
            [Frozen] Mock<IInputValidator<ChargePriceDto>> inputValidator,
            [Frozen] Mock<IBusinessValidator<ChargePriceDto>> businessValidator,
            [Frozen] Mock<IChargeCommandReceiptService> receiptService,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            TestMarketParticipant sender,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            ChargeBuilder chargeBuilder,
            ChargePriceDtoBuilder chargePriceDtoBuilder,
            ChargeCommandBuilder chargeCommandBuilder,
            ChargePriceEventHandler sut)
        {
            // Arrange
            var chargePriceDto = chargePriceDtoBuilder.WithPoints(BuildPoints()).Build();
            var chargeCommand = chargeCommandBuilder.WithChargeOperation(chargePriceDto).Build();
            var receivedEvent = new ChargeCommandReceivedEvent(Instant.MinValue, chargeCommand);

            var validationResult = GetFailedValidationResult();
            SetupValidators(inputValidator, businessValidator, validationResult);
            var rejected = false;
            var charge = chargeBuilder.Build();
            SetupChargeRepository(chargeRepository, charge);
            SetupMarketParticipantRepository(marketParticipantRepository, sender);
            SetupChargeIdentifierFactoryMock(chargeIdentifierFactory);

            receiptService
                .Setup(s => s.RejectInvalidOperationsAsync(
                        It.IsAny<IReadOnlyCollection<IChargeOperation>>(),
                        It.IsAny<DocumentDto>(),
                        It.IsAny<IList<IValidationRuleContainer>>()))
                .Callback<IReadOnlyCollection<IChargeOperation>, DocumentDto, IList<IValidationRuleContainer>>((_, _, _) => rejected = true);

            // Act
            await sut.HandleAsync(receivedEvent);

            // Assert
            rejected.Should().Be(true);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_IfEventIsNull_ThrowsArgumentNullException(
            ChargePriceEventHandler sut)
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
            [Frozen] Mock<IInputValidator<ChargePriceDto>> inputValidator,
            [Frozen] Mock<IBusinessValidator<ChargePriceDto>> businessValidator,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            TestMarketParticipant sender,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            ChargeBuilder chargeBuilder,
            ChargePriceEventHandler sut)
        {
            // Arrange
            var validationResult = ValidationResult.CreateSuccess();
            SetupValidators(inputValidator, businessValidator, validationResult);
            var points = new List<Point>();
            var price = 99.00M;
            for (var i = 1; i <= 24; i++)
            {
                points.Add(new Point(i, price + i, InstantHelper.GetTodayPlusDaysAtMidnightUtc(i)));
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
        public async Task HandleAsync_WhenValidationFailsInBundleOperation_RejectEventForAllSubsequentOperations(
            [Frozen] Mock<IChargeIdentifierFactory> chargeIdentifierFactory,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            [Frozen] Mock<IDocumentValidator<ChargeCommand>> documentValidator,
            [Frozen] Mock<IInputValidator<ChargePriceDto>> inputValidator,
            [Frozen] Mock<IBusinessValidator<ChargePriceDto>> businessValidator,
            [Frozen] Mock<IChargeCommandReceiptService> receiptService,
            TestMarketParticipant sender,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            ChargeBuilder chargeBuilder,
            ChargePriceEventHandler sut)
         {
             // Arrange
             var receivedEvent = CreateReceivedEventWithChargeOperations();
             var charge = chargeBuilder.Build();
             SetupChargeRepository(chargeRepository, charge);
             SetupMarketParticipantRepository(marketParticipantRepository, sender);
             SetupChargeIdentifierFactoryMock(chargeIdentifierFactory);

             var invalidValidationResult = ValidationResult.CreateFailure(
                 new List<IValidationRuleContainer>
                 {
                     new DocumentValidationRuleContainer(
                         new TestValidationRule(false, ValidationRuleIdentifier.StartDateValidation)),
                 });

             SetupValidatorsForOperation(documentValidator, inputValidator, businessValidator, invalidValidationResult);

             var accepted = 0;
             receiptService
                 .Setup(s => s.AcceptValidOperationsAsync(
                     It.IsAny<IReadOnlyCollection<IChargeOperation>>(),
                     It.IsAny<DocumentDto>()))
                 .Callback<IReadOnlyCollection<IChargeOperation>, DocumentDto>((_, _) => accepted++);
             var rejectedRules = new List<IValidationRuleContainer>();
             receiptService
                 .Setup(s => s.RejectInvalidOperationsAsync(
                     It.IsAny<IReadOnlyCollection<IChargeOperation>>(),
                     It.IsAny<DocumentDto>(),
                     It.IsAny<IList<IValidationRuleContainer>>()))
                 .Callback<IReadOnlyCollection<IChargeOperation>, DocumentDto, IList<IValidationRuleContainer>>((_, _, s) =>
                     rejectedRules.AddRange(s));

             // Act
             await sut.HandleAsync(receivedEvent);

             // Assert
             accepted.Should().Be(1);
             var invalid = rejectedRules.Where(vr =>
                 vr.ValidationRule.ValidationRuleIdentifier == ValidationRuleIdentifier.StartDateValidation);
             var subsequent = rejectedRules.Where(vr =>
                 vr.ValidationRule.ValidationRuleIdentifier == ValidationRuleIdentifier.SubsequentBundleOperationsFail);

             rejectedRules.Count.Should().Be(3);
             invalid.Count().Should().Be(1);
             subsequent.Count().Should().Be(2);
         }

        private static List<Point> BuildPoints()
        {
            var points = new List<Point>
            {
                new(0, 1.00m, InstantHelper.GetTodayPlusDaysAtMidnightUtc(0)),
                new(1, 2.00m, InstantHelper.GetTodayPlusDaysAtMidnightUtc(1)),
            };
            return points;
        }

        private static ChargeCommandReceivedEvent CreateReceivedEventWithChargeOperations()
        {
            var validChargeInformationDto = new ChargePriceDtoBuilder()
                .WithChargeId("valid")
                .WithStartDateTime(InstantHelper.GetYesterdayAtMidnightUtc())
                .WithEndDateTime(InstantHelper.GetEndDefault())
                .Build();
            var invalidChargeInformationDto = new ChargePriceDtoBuilder()
                .WithChargeId("invalid")
                .WithStartDateTime(InstantHelper.GetYesterdayAtMidnightUtc())
                .WithEndDateTime(InstantHelper.GetEndDefault())
                .Build();
            var failedChargeInformationDto = new ChargePriceDtoBuilder()
                .WithChargeId("failed")
                .WithStartDateTime(InstantHelper.GetYesterdayAtMidnightUtc())
                .WithEndDateTime(InstantHelper.GetEndDefault())
                .Build();
            var chargeCommand = new ChargeCommandBuilder()
                .WithChargeOperations(
                    new List<IChargeOperation>
                    {
                        validChargeInformationDto,
                        invalidChargeInformationDto,
                        failedChargeInformationDto,
                        failedChargeInformationDto,
                    })
                .Build();
            var receivedEvent = new ChargeCommandReceivedEvent(
                InstantHelper.GetTodayAtMidnightUtc(),
                chargeCommand);
            return receivedEvent;
        }

        private static void SetupValidatorsForOperation(
            Mock<IDocumentValidator<ChargeCommand>> documentValidator,
            Mock<IInputValidator<ChargePriceDto>> inputValidator,
            Mock<IBusinessValidator<ChargePriceDto>> businessValidator,
            ValidationResult invalidValidationResult)
        {
            inputValidator
                .Setup(v => v.Validate(It.IsAny<ChargePriceDto>()))
                .Returns(ValidationResult.CreateSuccess());

            documentValidator
                .Setup(v => v.ValidateAsync(It.IsAny<ChargeCommand>()))
                .ReturnsAsync(ValidationResult.CreateSuccess());

            businessValidator
                .Setup(v => v.ValidateAsync(It.Is<ChargePriceDto>(x => x.ChargeId == "valid")))
                .Returns(Task.FromResult(ValidationResult.CreateSuccess()));

            businessValidator
                .Setup(v => v.ValidateAsync(It.Is<ChargePriceDto>(x => x.ChargeId == "invalid")))
                .Returns(Task.FromResult(invalidValidationResult));
        }

        private static void SetupMarketParticipantRepository(
            Mock<IMarketParticipantRepository> marketParticipantRepository,
            TestMarketParticipant marketParticipant)
        {
            marketParticipantRepository
                .Setup(r => r.SingleAsync(It.IsAny<string>()))
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
            Mock<IInputValidator<ChargePriceDto>> inputValidator,
            Mock<IBusinessValidator<ChargePriceDto>> businessValidator,
            ValidationResult validationResult)
        {
            inputValidator.Setup(v => v.Validate(It.IsAny<ChargePriceDto>())).Returns(validationResult);
            businessValidator.Setup(v => v.ValidateAsync(It.IsAny<ChargePriceDto>()))
                .Returns(Task.FromResult(validationResult));
        }

        private static ValidationResult GetFailedValidationResult()
        {
            var failedRule = new Mock<IValidationRule>();
            failedRule.Setup(r => r.IsValid).Returns(false);

            return ValidationResult.CreateFailure(
                new List<IValidationRuleContainer> { new DocumentValidationRuleContainer(failedRule.Object) });
        }

        private static ChargeCommand CreateChargeCommandWith24Points()
        {
            var points = new List<Point>();
            var price = 0.00M;
            for (var i = 1; i <= 24; i++)
            {
                points.Add(new Point(i, price + i, InstantHelper.GetTodayPlusDaysAtMidnightUtc(i)));
            }

            var operation = new ChargePriceDtoBuilder()
                .WithPoints(points)
                .Build();

            return new ChargeCommandBuilder()
                .WithChargeOperation(operation)
                .Build();
        }
    }
}
