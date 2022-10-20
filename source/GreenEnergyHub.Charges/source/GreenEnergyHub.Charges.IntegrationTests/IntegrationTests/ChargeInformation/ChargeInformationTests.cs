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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.Core.JsonSerialization;
using GreenEnergyHub.Charges.Application.Charges.Handlers.ChargeInformation;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.FunctionHost;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.Database;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.TestCore.Builders.Command;
using Moq;
using Xunit;
using Xunit.Categories;
using JsonSerializer = Energinet.DataHub.Core.JsonSerialization.JsonSerializer;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.ChargeInformation
{
    /// <summary>
    /// Tests ChargeInformation flow with managed dependencies
    /// </summary>
    [IntegrationTest]
    public class ChargeInformationTests : IClassFixture<ChargesDatabaseFixture>
    {
        private readonly ChargesDatabaseManager _databaseManager;
        private readonly IJsonSerializer _jsonSerializer;

        public ChargeInformationTests(ChargesDatabaseFixture fixture)
        {
            _databaseManager = fixture.DatabaseManager;
            _jsonSerializer = new JsonSerializer();
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_WhenValidationFails_ThenRejectedEventRaised(
            ChargeInformationOperationsHandler sut)
        {
            // Arrange
            ChargesFunctionApp.ConfigureApplication();
            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();
            var jsonString = await File.ReadAllTextAsync("IntegrationTests\\ChargeInformation\\ChargeInformationTestTariff.json");
            try
            {
                var receivedEvent = _jsonSerializer.Deserialize<ChargeInformationCommandReceivedEvent>(jsonString);

                // Act
                await sut.HandleAsync(receivedEvent);

                // Assert
                var senderProvidedChargeId = receivedEvent.Command.Operations.First().SenderProvidedChargeId;
                var charge = chargesDatabaseWriteContext.Charges.Single(x => x.Id.ToString() == senderProvidedChargeId);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        /*[Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_IfValidUpdateEvent_ChargeUpdated(
            [Frozen] Mock<IChargeIdentifierFactory> chargeIdentifierFactory,
            [Frozen] Mock<IChargePeriodFactory> chargePeriodFactory,
            [Frozen] Mock<IInputValidator<ChargeInformationOperationDto>> inputValidator,
            [Frozen] Mock<IChargeInformationOperationsAcceptedEventFactory> chargeInformationOperationsAcceptedEventFactory,
            ChargeInformationOperationsAcceptedEventBuilder chargeInformationOperationsAcceptedEventBuilder,
            ChargeBuilder chargeBuilder,
            ChargePeriodBuilder chargePeriodBuilder,
            ChargeInformationCommandBuilder chargeInformationCommandBuilder,
            ChargeInformationOperationDtoBuilder chargeInformationOperationDtoBuilder,
            ChargeInformationOperationsHandler sut)
        {
            // Arrange
            var charge = chargeBuilder.Build();
            var updateOperationDto = chargeInformationOperationDtoBuilder
                .WithStartDateTime(InstantHelper.GetTodayAtMidnightUtc())
                .Build();
            var chargeCommand = chargeInformationCommandBuilder.WithChargeOperation(updateOperationDto).Build();
            var receivedEvent = new ChargeInformationCommandReceivedEvent(InstantHelper.GetTodayAtMidnightUtc(), chargeCommand);
            var validationResult = ValidationResult.CreateSuccess();
            SetupValidators(inputValidator, validationResult);
            var newPeriod = chargePeriodBuilder
                .WithStartDateTime(updateOperationDto.StartDateTime)
                .Build();
            SetupChargeIdentifierFactoryMock(chargeIdentifierFactory);
            chargePeriodFactory.Setup(cpf => cpf.CreateFromChargeOperationDto(updateOperationDto))
                .Returns(newPeriod);
            var chargeInformationOperationsAcceptedEvent = chargeInformationOperationsAcceptedEventBuilder.Build();
            chargeInformationOperationsAcceptedEventFactory
                .Setup(c => c.Create(
                    It.IsAny<DocumentDto>(),
                    It.IsAny<IReadOnlyCollection<ChargeInformationOperationDto>>()))
                .Returns(chargeInformationOperationsAcceptedEvent);

            // Act
            await sut.HandleAsync(receivedEvent);

            // Assert
            var timeline = charge.Periods.OrderBy(p => p.StartDateTime).ToList();
            var firstPeriod = timeline[0];
            var secondPeriod = timeline[1];

            firstPeriod.StartDateTime.Should().Be(InstantHelper.GetStartDefault());
            firstPeriod.EndDateTime.Should().Be(newPeriod.StartDateTime);
            secondPeriod.StartDateTime.Should().Be(newPeriod.StartDateTime);
            secondPeriod.EndDateTime.Should().Be(newPeriod.EndDateTime);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_IfValidStopEvent_ChargeStopped(
            [Frozen] Mock<IChargeIdentifierFactory> chargeIdentifierFactory,
            [Frozen] Mock<IInputValidator<ChargeInformationOperationDto>> inputValidator,
            [Frozen] Mock<IChargeInformationOperationsAcceptedEventFactory> chargeInformationOperationsAcceptedEventFactory,
            ChargeInformationOperationsAcceptedEventBuilder chargeInformationOperationsAcceptedEventBuilder,
            ChargeBuilder chargeBuilder,
            ChargeInformationCommandBuilder chargeInformationCommandBuilder,
            ChargeInformationOperationDtoBuilder chargeInformationOperationDtoBuilder,
            ChargeInformationOperationsHandler sut)
        {
            // Arrange
            var stopDate = InstantHelper.GetTodayAtMidnightUtc();
            var stopOperationDto = chargeInformationOperationDtoBuilder.WithStartDateTime(stopDate).WithEndDateTime(stopDate).Build();
            var chargeCommand = chargeInformationCommandBuilder.WithChargeOperation(stopOperationDto).Build();
            var receivedEvent = new ChargeInformationCommandReceivedEvent(InstantHelper.GetTodayAtMidnightUtc(), chargeCommand);
            var charge = chargeBuilder.Build();
            var validationResult = ValidationResult.CreateSuccess();
            SetupValidators(inputValidator, validationResult);
            SetupChargeIdentifierFactoryMock(chargeIdentifierFactory);
            var chargeInformationOperationsAcceptedEvent = chargeInformationOperationsAcceptedEventBuilder.Build();
            chargeInformationOperationsAcceptedEventFactory
                .Setup(c => c.Create(
                    It.IsAny<DocumentDto>(),
                    It.IsAny<IReadOnlyCollection<ChargeInformationOperationDto>>()))
                .Returns(chargeInformationOperationsAcceptedEvent);

            // Act
            await sut.HandleAsync(receivedEvent);

            // Assert
            charge.Periods.Count.Should().Be(1);
            var actual = charge.Periods.Single();
            actual.EndDateTime.Should().Be(stopDate);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_WhenValidCancelStop_ThenStopCancelled(
            [Frozen] Mock<IChargeIdentifierFactory> chargeIdentifierFactory,
            [Frozen] Mock<IInputValidator<ChargeInformationOperationDto>> inputValidator,
            [Frozen] Mock<IChargePeriodFactory> chargePeriodFactory,
            [Frozen] Mock<IChargeInformationOperationsAcceptedEventFactory> chargeInformationOperationsAcceptedEventFactory,
            ChargeInformationOperationsAcceptedEventBuilder chargeInformationOperationsAcceptedEventBuilder,
            ChargeInformationCommandBuilder chargeInformationCommandBuilder,
            ChargeBuilder chargeBuilder,
            ChargeInformationOperationDtoBuilder chargeInformationOperationDtoBuilder,
            ChargeInformationOperationsHandler sut)
        {
            // Arrange
            var validationResult = ValidationResult.CreateSuccess();
            SetupValidators(inputValidator, validationResult);
            var chargeOperationDto = chargeInformationOperationDtoBuilder
                .WithStartDateTime(InstantHelper.GetTomorrowAtMidnightUtc())
                .WithEndDateTime(InstantHelper.GetEndDefault())
                .Build();
            var chargeCommand = chargeInformationCommandBuilder
                .WithChargeOperation(chargeOperationDto)
                .Build();
            var receivedEvent = new ChargeInformationCommandReceivedEvent(InstantHelper.GetTodayAtMidnightUtc(), chargeCommand);
            var charge = chargeBuilder.WithStopDate(InstantHelper.GetTomorrowAtMidnightUtc()).Build();
            SetupChargeIdentifierFactoryMock(chargeIdentifierFactory);
            SetupChargePeriodFactory(chargePeriodFactory);
            var chargeInformationOperationsAcceptedEvent = chargeInformationOperationsAcceptedEventBuilder.Build();
            chargeInformationOperationsAcceptedEventFactory
                .Setup(c => c.Create(
                    It.IsAny<DocumentDto>(),
                    It.IsAny<IReadOnlyCollection<ChargeInformationOperationDto>>()))
                .Returns(chargeInformationOperationsAcceptedEvent);

            // Act
            await sut.HandleAsync(receivedEvent);

            // Assert
            charge.Periods.Count.Should().Be(2);
            var actual = charge.Periods.OrderByDescending(p => p.StartDateTime).First();
            actual.StartDateTime.Should().Be(InstantHelper.GetTomorrowAtMidnightUtc());
            actual.EndDateTime.Should().Be(InstantHelper.GetEndDefault());
        }*/

        private static void SetupChargeIdentifierFactoryMock(Mock<IChargeIdentifierFactory> chargeIdentifierFactory)
        {
            chargeIdentifierFactory
                .Setup(x => x.CreateAsync(
                    It.IsAny<string>(),
                    It.IsAny<ChargeType>(),
                    It.IsAny<string>()))
                .ReturnsAsync(It.IsAny<ChargeIdentifier>());
        }

        private static void SetupChargePeriodFactory(Mock<IChargePeriodFactory> chargePeriodFactory, ChargePeriod? period = null)
        {
            if (period is null)
            {
                period = new ChargePeriodBuilder()
                    .WithStartDateTime(InstantHelper.GetTomorrowAtMidnightUtc())
                    .Build();
            }

            chargePeriodFactory
                .Setup(r => r.CreateFromChargeOperationDto(It.IsAny<ChargeInformationOperationDto>()))
                .Returns(period);
        }

        private static (ChargeInformationCommandReceivedEvent ReceivedEvent, string InvalidOperationId) CreateReceivedEventWithChargeOperations()
        {
            var validChargeOperationDto = new ChargeInformationOperationDtoBuilder()
                .WithChargeOperationId("Operation1")
                .WithStartDateTime(InstantHelper.GetYesterdayAtMidnightUtc())
                .Build();
            var invalidChargeOperationDto = new ChargeInformationOperationDtoBuilder()
                .WithChargeOperationId("Operation2")
                .WithTaxIndicator(TaxIndicator.NoTax)
                .WithStartDateTime(InstantHelper.GetYesterdayAtMidnightUtc())
                .Build();
            var failedChargeOperationDto = new ChargeInformationOperationDtoBuilder()
                .WithChargeOperationId("Operation3")
                .WithStartDateTime(InstantHelper.GetYesterdayAtMidnightUtc())
                .Build();
            var anotherFailedChargeOperationDto = new ChargeInformationOperationDtoBuilder()
                .WithChargeOperationId("Operation4")
                .WithStartDateTime(InstantHelper.GetYesterdayAtMidnightUtc())
                .Build();
            var chargeCommand = new ChargeInformationCommandBuilder()
                .WithChargeOperations(
                    new List<ChargeInformationOperationDto>
                    {
                        validChargeOperationDto,
                        invalidChargeOperationDto,
                        failedChargeOperationDto,
                        anotherFailedChargeOperationDto,
                    })
                .Build();
            var receivedEvent = new ChargeInformationCommandReceivedEventBuilder()
                .WithCommand(chargeCommand)
                .Build();
            return (ReceivedEvent: receivedEvent, InvalidOperationId: invalidChargeOperationDto.OperationId);
        }

        private static ValidationResult GetFailedValidationResult()
        {
            var failedRule = new Mock<IValidationRule>();
            failedRule.Setup(r => r.IsValid).Returns(false);

            return ValidationResult.CreateFailure(
                new List<IValidationRuleContainer> { new DocumentValidationRuleContainer(failedRule.Object) });
        }

        private static void SetupValidators(
            Mock<IInputValidator<ChargeInformationOperationDto>> inputValidator,
            ValidationResult validationResult)
        {
            inputValidator
                .Setup(v => v.Validate(
                    It.IsAny<ChargeInformationOperationDto>(), It.IsAny<DocumentDto>()))
                .Returns(validationResult);
        }
    }
}
