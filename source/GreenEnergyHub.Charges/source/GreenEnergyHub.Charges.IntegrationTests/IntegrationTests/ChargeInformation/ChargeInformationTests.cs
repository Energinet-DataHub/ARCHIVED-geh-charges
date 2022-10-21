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

using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.Core.JsonSerialization;
using FluentAssertions;
using GreenEnergyHub.Charges.Application.Charges.Factories;
using GreenEnergyHub.Charges.Application.Charges.Handlers.ChargeInformation;
using GreenEnergyHub.Charges.Application.Common.Services;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.Database;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.ChargeInformation
{
    /// <summary>
    /// Tests ChargeInformation flow with managed dependencies
    /// </summary>
    [IntegrationTest]
    public class ChargeInformationTests : IClassFixture<ChargesManagedDependenciesTestFixture>
    {
        private readonly ChargesManagedDependenciesTestFixture _fixture;
        private readonly IJsonSerializer _jsonSerializer;

        public ChargeInformationTests(ChargesManagedDependenciesTestFixture fixture)
        {
            _fixture = fixture;
            _jsonSerializer = _fixture.GetService<IJsonSerializer>();
        }

        [Fact]
        public async Task HandleAsync_WhenValidChargeInformationCommandReceivedEvent_ThenChargeIsPersisted()
        {
            // Arrange
            await using var chargesDatabaseReadContext = _fixture.DatabaseManager.CreateDbContext();
            var sut = BuildChargeInformationOperationsHandler();
            var receivedEvent =
                await GetTestDataFromFile<ChargeInformationCommandReceivedEvent>("ChargeInformationTestTariff.json");

            // Act
            await sut.HandleAsync(receivedEvent);
            await _fixture.UnitOfWork.SaveChangesAsync();

            // Assert
            var senderProvidedChargeId = receivedEvent.Command.Operations.First().SenderProvidedChargeId;
            var charge = chargesDatabaseReadContext.Charges
                .First(x => x.SenderProvidedChargeId == senderProvidedChargeId);

            charge.Should().NotBeNull();
        }

        private async Task<T> GetTestDataFromFile<T>(string filename)
        {
            var jsonString = await File.ReadAllTextAsync($"IntegrationTests\\ChargeInformation\\{filename}");
            return _jsonSerializer.Deserialize<T>(jsonString);
        }

        private ChargeInformationOperationsHandler BuildChargeInformationOperationsHandler()
        {
            return new ChargeInformationOperationsHandler(
                _fixture.GetService<IInputValidator<ChargeInformationOperationDto>>(),
                _fixture.GetService<IChargeRepository>(),
                _fixture.GetService<IMarketParticipantRepository>(),
                _fixture.GetService<IChargeFactory>(),
                _fixture.GetService<IChargePeriodFactory>(),
                _fixture.GetService<IDomainEventPublisher>(),
                _fixture.GetService<ILoggerFactory>(),
                _fixture.GetService<IChargeInformationOperationsAcceptedEventFactory>(),
                _fixture.GetService<IChargeInformationOperationsRejectedEventFactory>());
        }
    }
}
