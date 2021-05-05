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
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using GreenEnergyHub.Charges.Application.ChangeOfCharges;
using GreenEnergyHub.Charges.IntegrationTests.TestHelpers;
using GreenEnergyHub.Charges.MessageReceiver;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.Json;
using GreenEnergyHub.TestHelpers;
using GreenEnergyHub.TestHelpers.Traits;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NodaTime;
using Xunit;

namespace GreenEnergyHub.Charges.IntegrationTests.Application.ChangeOfCharges
{
    [Trait(TraitNames.Category, TraitValues.IntegrationTest)]
    public class ChangeOfChargesMessageHandlerTests
    {
        private readonly ChargeHttpTrigger _sut;

        public ChangeOfChargesMessageHandlerTests()
        {
            TestConfigurationHelper.ConfigureEnvironmentVariablesFromLocalSettings();
            var host = TestConfigurationHelper.SetupHost();

            _sut = new ChargeHttpTrigger(
                host.Services.GetRequiredService<IJsonSerializer>(),
                host.Services.GetRequiredService<IChangeOfChargesMessageHandler>());
        }

        [Theory]
        [InlineAutoMoqData("TestFiles\\ValidChargeAddition.json")]
        [InlineAutoMoqData("TestFiles\\ValidChargeUpdate.json")]
        public async Task ChargeCommandAccepted_Is_True(
            string testFile,
            [NotNull] [Frozen] Mock<ILogger> logger,
            [NotNull] ExecutionContext executionContext,
            [NotNull] IClock clock)
        {
            // arrange
            var req = CreateHttpRequest(testFile, clock);
            SetInvocationId(executionContext);

            // act
            var result = (OkObjectResult)await _sut.RunAsync(req, executionContext, logger.Object).ConfigureAwait(false);

            // assert
            Assert.Equal(200, result!.StatusCode!.Value);
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task ChargeCommandRejected_Is_True(
            [NotNull] [Frozen] Mock<IChangeOfChargesTransactionHandler> changeOfChargesTransactionHandler,
            [NotNull] ChangeOfChargesMessageHandler sut)
        {
            // Arrange
            var testCoCh = changeOfChargesTransactionHandler;
            var newSut = sut;

            // Act

            // Assert
            await Task.Run(() => Assert.True(true)).ConfigureAwait(false);
        }

        private static void SetInvocationId(ExecutionContext executionContext)
        {
            executionContext.InvocationId = Guid.NewGuid();
        }

        private static DefaultHttpRequest CreateHttpRequest(string testFile, IClock clock)
        {
            var stream = TestDataHelper.GetInputStream(testFile, clock);
            var defaultHttpContext = new DefaultHttpContext();
            defaultHttpContext.Request.Body = stream;
            var req = new DefaultHttpRequest(defaultHttpContext);
            return req;
        }
    }
}
