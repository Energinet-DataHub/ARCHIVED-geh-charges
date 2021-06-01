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
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Domain.Acknowledgements;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Result;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using GreenEnergyHub.Charges.IntegrationTests.TestHelpers;
using GreenEnergyHub.Charges.TestCore;
using Microsoft.Azure.WebJobs;
using NodaTime;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;
using JsonSerializer = GreenEnergyHub.Charges.Core.Json.JsonSerializer;

namespace GreenEnergyHub.Charges.IntegrationTests.Application.ChangeOfCharges
{
    [IntegrationTest]
    public class ChangeOfChargePipelineTests : IClassFixture<DbContextRegistrator>
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly string _messageReceiverHostname;
        private readonly string _postOfficeSubscriptionName;
        private readonly string _postOfficeTopicName;
        private readonly string _postOfficeConnectionString;
        private readonly ChargeDbQueries _chargeDbQueries;

        public ChangeOfChargePipelineTests([NotNull] DbContextRegistrator dbContextRegistrator, ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _testOutputHelper.WriteLine($"{nameof(ChangeOfChargePipelineTests)} constructor invoked");

            _chargeDbQueries = new ChargeDbQueries(dbContextRegistrator.ServiceProvider);

            _messageReceiverHostname = Environment.GetEnvironmentVariable("MESSAGE_RECEIVER_HOSTNAME") !;
            _postOfficeSubscriptionName = Environment.GetEnvironmentVariable("POST_OFFICE_SUBSCRIPTION_NAME") !;
            _postOfficeTopicName = Environment.GetEnvironmentVariable("POST_OFFICE_TOPIC_NAME") !;
            _postOfficeConnectionString = Environment.GetEnvironmentVariable("POST_OFFICE_LISTENER_CONNECTION_STRING") !;

            _testOutputHelper.WriteLine($"{nameof(ChangeOfChargePipelineTests)} Configuration: " +
                                        $"{_messageReceiverHostname}," +
                                        $"{_postOfficeSubscriptionName}, " +
                                        $"{_postOfficeTopicName}");
        }

        [Theory(Timeout = 60000)]
        [InlineAutoMoqData("TestFiles/ValidCreateTariffCommand.json")]
        public async Task Test_ChargeCommandCompleteFlow_is_Accepted(
            string testFilePath,
            [NotNull] ExecutionContext executionContext,
            [NotNull] ServiceBusTestHelper serviceBusTestHelper)
        {
            _testOutputHelper.WriteLine($"Run {nameof(Test_ChargeCommandCompleteFlow_is_Accepted)} for CorrelationId: {executionContext.InvocationId}");

            // arrange
            IClock clock = SystemClock.Instance;
            var chargeJson = EmbeddedResourceHelper.GetInputJson(testFilePath, clock);
            var chargeCommand = new JsonSerializer().Deserialize<ChargeCommand>(chargeJson);

            _testOutputHelper.WriteLine($"ChargeCommand.Document.ID: {chargeCommand.Document.Id}");

            // act
            var changeOfChargesMessageResult = await RunMessageReceiver(chargeJson).ConfigureAwait(false);
            _testOutputHelper.WriteLine($"MessageReceiver response is succeeded: {changeOfChargesMessageResult.IsSucceeded}");

            var (receivedEvent, receivedMessage) = await serviceBusTestHelper
                .GetMessageFromServiceBusAsync<ChargeConfirmation>(
                    _postOfficeConnectionString,
                    _postOfficeTopicName,
                    _postOfficeSubscriptionName,
                    changeOfChargesMessageResult.CorrelationId!)
                .ConfigureAwait(false);

            _testOutputHelper.WriteLine($"CommandAcceptedMessage: {receivedMessage.CorrelationId}");

            var chargeExistsByCorrelationId = await _chargeDbQueries
                .ChargeExistsByCorrelationIdAsync(receivedMessage.CorrelationId)
                .ConfigureAwait(false);

            // assert
            Assert.True(changeOfChargesMessageResult.IsSucceeded);
            Assert.Equal(chargeCommand.Document.Id, receivedEvent.OriginalTransactionReferenceMRid);
            Assert.NotNull(receivedEvent);
            Assert.True(chargeExistsByCorrelationId);
        }

        [Theory(Timeout = 60000)]
        [InlineAutoMoqData("TestFiles/InvalidCreateTariffCommand.json")]
        public async Task Test_ChargeCommandCompleteFlow_is_Rejected(
            string testFilePath,
            [NotNull] ExecutionContext executionContext,
            [NotNull] ServiceBusTestHelper serviceBusTestHelper)
        {
            _testOutputHelper.WriteLine($"Run {nameof(Test_ChargeCommandCompleteFlow_is_Rejected)} for CorrelationId: {executionContext.InvocationId}");

            // arrange
            IClock clock = SystemClock.Instance;
            var chargeJson = EmbeddedResourceHelper.GetInputJson(testFilePath, clock);
            var chargeCommand = new JsonSerializer().Deserialize<ChargeCommand>(chargeJson);

            _testOutputHelper.WriteLine($"ChargeCommand.Document.ID: {chargeCommand.Document.Id}");

            // act
            var changeOfChargesMessageResult = await RunMessageReceiver(chargeJson).ConfigureAwait(false);
            _testOutputHelper.WriteLine($"MessageReceiver is succeeded: {changeOfChargesMessageResult.IsSucceeded}");

            var (receivedEvent, receivedMessage) = await serviceBusTestHelper
                .GetMessageFromServiceBusAsync<ChargeRejection>(
                    _postOfficeConnectionString,
                    _postOfficeTopicName,
                    _postOfficeSubscriptionName,
                    changeOfChargesMessageResult.CorrelationId!)
                .ConfigureAwait(false);

            _testOutputHelper.WriteLine($"CommandAcceptedMessage: {receivedMessage.CorrelationId}");

            var chargeExistsByCorrelationId = await _chargeDbQueries
                .ChargeExistsByCorrelationIdAsync(executionContext.InvocationId.ToString())
                .ConfigureAwait(false);

            // assert
            Assert.True(changeOfChargesMessageResult.IsSucceeded);
            Assert.Equal(chargeCommand.Document.Id, receivedEvent.OriginalTransactionReferenceMRid);
            Assert.NotNull(receivedEvent);
            Assert.False(chargeExistsByCorrelationId);
        }

        private async Task<ChangeOfChargesMessageResult> RunMessageReceiver([NotNull] string json)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var requestUri = new Uri($"https://{_messageReceiverHostname}/api/chargehttptrigger/");
            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(requestUri, stringContent).ConfigureAwait(false);
            stringContent.Dispose();

            string responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return new JsonSerializer().Deserialize<ChangeOfChargesMessageResult>(responseContent);
        }
    }
}
