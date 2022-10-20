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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;
using Energinet.DataHub.Core.FunctionApp.TestCommon.FunctionAppHost;
using Energinet.DataHub.Core.JsonSerialization;
using FluentAssertions;
using GreenEnergyHub.Charges.Application.Charges.Factories;
using GreenEnergyHub.Charges.Application.Charges.Handlers.ChargeInformation;
using GreenEnergyHub.Charges.Application.Common.Services;
using GreenEnergyHub.Charges.Application.Persistence;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.FunctionHost;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.Database;
using GreenEnergyHub.Charges.TestCore.TestHelpers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Categories;

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
        private readonly IHost _host;
        private readonly IUnitOfWork _unitOfWork;

        public ChargeInformationTests(ChargesDatabaseFixture fixture)
        {
            _databaseManager = fixture.DatabaseManager;
            _jsonSerializer = new JsonSerializer();
            var configuration = new FunctionAppHostConfigurationBuilder().BuildLocalSettingsConfiguration();
            FunctionHostEnvironmentSettingHelper.SetFunctionHostEnvironmentVariablesFromSampleSettingsFile(configuration);
            Environment.SetEnvironmentVariable("CHARGE_DB_CONNECTION_STRING", _databaseManager.ConnectionString);
            _host = ChargesFunctionApp.ConfigureApplication();
            _unitOfWork = (IUnitOfWork)_host.Services.GetService(typeof(IUnitOfWork))!;
            var correlationContext = (ICorrelationContext)_host.Services.GetService(typeof(ICorrelationContext))!;
            correlationContext.SetId(Guid.NewGuid().ToString());
        }

        [Fact]
        public async Task HandleAsync_WhenValidationFails_ThenRejectedEventRaised()
        {
            // Arrange
            await using var chargesDatabaseReadContext = _databaseManager.CreateDbContext();
            var sut = BuildChargeInformationOperationsHandler();
            var jsonString = await File.ReadAllTextAsync("IntegrationTests\\ChargeInformation\\ChargeInformationTestTariff.json");

            var receivedEvent = _jsonSerializer.Deserialize<ChargeInformationCommandReceivedEvent>(jsonString);

            // Act
            await sut.HandleAsync(receivedEvent);
            await _unitOfWork.SaveChangesAsync();

            // Assert
            var senderProvidedChargeId = receivedEvent.Command.Operations.First().SenderProvidedChargeId;
            var charge = chargesDatabaseReadContext.Charges
                .First(x => x.SenderProvidedChargeId == senderProvidedChargeId);

            charge.Should().NotBeNull();
        }

        private ChargeInformationOperationsHandler BuildChargeInformationOperationsHandler()
        {
            return new ChargeInformationOperationsHandler(
                GetService<IInputValidator<ChargeInformationOperationDto>>(),
                GetService<IChargeRepository>(),
                GetService<IMarketParticipantRepository>(),
                GetService<IChargeFactory>(),
                GetService<IChargePeriodFactory>(),
                GetService<IDomainEventPublisher>(),
                GetService<ILoggerFactory>(),
                GetService<IChargeInformationOperationsAcceptedEventFactory>(),
                GetService<IChargeInformationOperationsRejectedEventFactory>());
        }

        private T GetService<T>()
        {
            return (T)_host.Services.GetService(typeof(T))!;
        }
    }
}
