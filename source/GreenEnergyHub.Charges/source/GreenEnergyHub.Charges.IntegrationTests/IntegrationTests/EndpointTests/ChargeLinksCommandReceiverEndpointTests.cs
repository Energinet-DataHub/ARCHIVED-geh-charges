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
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksAcceptedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands.Validation.BusinessValidation.ValidationRules;
using GreenEnergyHub.Charges.FunctionHost.ChargeLinks.MessageHub;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.FunctionApp;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestCommon;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestHelpers;
using GreenEnergyHub.Charges.IntegrationTests.Fixtures;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.EndpointTests
{
    [IntegrationTest]
    public class ChargeLinksCommandReceiverEndpointTests
    {
        [Collection(nameof(ChargesFunctionAppCollectionFixture))]
        public class RunAsync : FunctionAppTestBase<ChargesFunctionAppFixture>, IAsyncLifetime
        {
            public RunAsync(ChargesFunctionAppFixture fixture, ITestOutputHelper testOutputHelper)
                : base(fixture, testOutputHelper)
            {
            }

            public Task DisposeAsync()
            {
                // We need to clear host log after each test is done to ensure that we can assert
                // on function executed on each test run because we only check on function name.
                Fixture.HostManager.ClearHostLog();
                Fixture.MessageHubMock.Clear();
                return Task.CompletedTask;
            }

            public Task InitializeAsync()
            {
                return Task.CompletedTask;
            }

            /// <summary>
            /// The attempt to update charge link (in this case using the same command) is not allowed for now and
            /// must be rejected by <see cref="ChargeLinksUpdateNotYetSupportedRule"/>. When charge link update is
            /// allowed and this rule is removed, this test should be removed, too.
            /// </summary>
            /// <param name="chargeLinksCommandBuilder"></param>
            /// <param name="chargeLinkDtoBuilder"></param>
            [Theory]
            [InlineAutoMoqData]
            public async Task When_ChargeLinksUpdateAttempted_Then_CreateRejection(
                ChargeLinksCommandBuilder chargeLinksCommandBuilder,
                ChargeLinkDtoBuilder chargeLinkDtoBuilder)
            {
                // Arrange
                await using var context = Fixture.ChargesDatabaseManager.CreateDbContext();
                var charge = await context.Charges.FirstAsync();
                var marketParticipant = await context.MarketParticipants.SingleAsync(mp => mp.Id == charge.OwnerId);

                var link = chargeLinkDtoBuilder
                    .WithMeteringPointId("571313180000000005")
                    .WithCharge(charge.SenderProvidedChargeId, charge.Type, marketParticipant.MarketParticipantId)
                    .Build();

                var links = new List<ChargeLinkDto> { link };
                var command = chargeLinksCommandBuilder.WithChargeLinks(links).Build();
                var correlationIdOne = CorrelationIdGenerator.Create();
                var parentIdOne = $"00-{correlationIdOne}-b7ad6b7169203331-02";
                var messageOne = CreateServiceBusMessage(command, correlationIdOne);

                // Act
                await MockTelemetryClient.WrappedOperationWithTelemetryDependencyInformationAsync(
                    () => Fixture.ChargeLinksReceivedTopic.SenderClient.SendMessageAsync(messageOne), correlationIdOne, parentIdOne);

                await FunctionAsserts.AssertHasExecutedAsync(
                    Fixture.HostManager, nameof(ChargeLinkConfirmationDataAvailableNotifierEndpoint));

                var correlationIdTwo = CorrelationIdGenerator.Create();
                var parentIdTwo = $"00-{correlationIdTwo}-b7ad6b7169203331-02";
                var messageTwo = CreateServiceBusMessage(command, correlationIdTwo);

                await MockTelemetryClient.WrappedOperationWithTelemetryDependencyInformationAsync(
                    () => Fixture.ChargeLinksReceivedTopic.SenderClient.SendMessageAsync(messageTwo), correlationIdTwo, parentIdTwo);

                // Assert
                await FunctionAsserts.AssertHasExecutedAsync(
                    Fixture.HostManager, nameof(ChargeLinksRejectionDataAvailableNotifierEndpoint));
            }

            private static ServiceBusMessage CreateServiceBusMessage(ChargeLinksCommand command, string correlationId)
            {
                var chargeLinksAcceptedEvent = new ChargeLinksAcceptedEvent(
                    command, Instant.FromDateTimeUtc(DateTime.UtcNow));

                var applicationProperties = new Dictionary<string, string>();
                var message = ServiceBusMessageGenerator.CreateWithJsonContent(
                    chargeLinksAcceptedEvent, applicationProperties, correlationId);

                return message;
            }
        }
    }
}
