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

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Energinet.DataHub.Core.JsonSerialization;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using FluentAssertions;
using GreenEnergyHub.Charges.Application.Charges.Handlers.ChargeInformation;
using GreenEnergyHub.Charges.Application.Persistence;
using GreenEnergyHub.Charges.FunctionHost.Charges;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.Charges;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions.Serialization;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures;
using GreenEnergyHub.Charges.TestCore.Builders.Command;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.EndpointTests.Charges
{
    [Collection(nameof(ChargeHistoryPersisterEndpointTests))]
    [IntegrationTest]
    public class ChargeHistoryPersisterEndpointTests
    {
        public class RunAsync : IClassFixture<ChargesManagedDependenciesTestFixture>
        {
            private readonly ChargesManagedDependenciesTestFixture _fixture;

            public RunAsync(ChargesManagedDependenciesTestFixture fixture)
            {
                _fixture = fixture;
            }

            [Theory]
            [InlineAutoMoqData]
            public async Task RunAsync_WhenChargeInformationOperationsAcceptedEvent_ChargeHistoryIsPersisted(
                ChargeInformationOperationsAcceptedEventBuilder chargeInformationOperationsAcceptedEventBuilder)
            {
                // Arrange
                var operationsAcceptedEvent = chargeInformationOperationsAcceptedEventBuilder.Build();
                var expectedOperations = operationsAcceptedEvent.Operations.ToList();
                var firstOperationDto = expectedOperations.First();
                var senderProvidedChargeId = firstOperationDto.SenderProvidedChargeId;
                var chargeType = firstOperationDto.ChargeType;
                var chargeOwner = firstOperationDto.ChargeOwner;
                var sut = new ChargeHistoryPersisterEndpoint(
                    _fixture.GetService<JsonMessageDeserializer>(),
                    _fixture.GetService<IChargeHistoryPersister>(),
                    _fixture.GetService<IUnitOfWork>());

                var jsonSerializer = new JsonSerializer();
                var dataString = jsonSerializer.Serialize(operationsAcceptedEvent);
                var data = Encoding.UTF8.GetBytes(dataString);

                // Act
                await sut.RunAsync(data);

                // Assert
                await using var chargesQueryDatabaseReadContext = _fixture.ChargesDatabaseManager.CreateDbContext();
                var actualHistories = chargesQueryDatabaseReadContext.ChargeHistories
                    .Where(x =>
                        x.SenderProvidedChargeId == senderProvidedChargeId &&
                        x.Type == chargeType &&
                        x.Owner == chargeOwner).ToList();
                actualHistories.Should().HaveSameCount(operationsAcceptedEvent.Operations);
                for (var i = 0; i < actualHistories.Count - 1; i++)
                {
                    var actualHistory = actualHistories[i];
                    var expectedOperation = expectedOperations[i];
                    actualHistory.Name.Should().Be(expectedOperation.ChargeName);
                    actualHistory.Description.Should().Be(expectedOperation.ChargeDescription);
                    actualHistory.Resolution.Should().Be(expectedOperation.Resolution);
                    actualHistory.TaxIndicator.Should().Be(TaxIndicatorMapper.Map(expectedOperation.TaxIndicator));
                    actualHistory.TransparentInvoicing.Should().Be(TransparentInvoicingMapper.Map(expectedOperation.TransparentInvoicing));
                    actualHistory.VatClassification.Should().Be(expectedOperation.VatClassification);
                    actualHistory.StartDateTime.Should().Be(expectedOperation.StartDateTime);
                    actualHistory.EndDateTime.Should().Be(expectedOperation.EndDateTime);
                    actualHistory.AcceptedDateTime.Should().Be(operationsAcceptedEvent.PublishedTime);
                }
            }
        }
    }
}
