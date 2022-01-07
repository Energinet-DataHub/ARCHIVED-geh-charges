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
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Energinet.DataHub.Core.TestCommon;
using FluentAssertions;
using GreenEnergyHub.Charges.SystemTests.Fixtures;

namespace GreenEnergyHub.Charges.SystemTests
{
    /// <summary>
    /// Contains tests where we operate at the level of a domain, so basically what in some context has been named "domain tests".
    /// However, with the technique displayed here we perform these tests in a live environment.
    /// Maybe we should only use this for testing domain health checks.
    /// </summary>
    public class MyDomainTests
    {
        public MyDomainTests()
        {
            Configuration = new MyDomainConfiguration();
        }

        private MyDomainConfiguration Configuration { get; }

#pragma warning disable VSTHRD200 // Use "Async" suffix for async methods
        [SystemFact(Skip = "Hi Peter, pls update this test for your scenario. :o)")]
        public async Task When_TriggeringAction_Then_PeekReturnsExpectedContent()
        {
            // Arrange
            using var httpClient = new HttpClient
            {
                BaseAddress = Configuration.BaseAddress,
            };

            // Flow
            // => Trigger action
            using var actionResponse = await httpClient.PostAsync("api/myActionEndpoint", new StringContent("myActionBody"));
            actionResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);

            // => Peek
            HttpResponseMessage? expectedResponse = null;
            await Awaiter.WaitUntilConditionAsync(
                async () =>
                {
                    expectedResponse = await httpClient.GetAsync("api/myPeekEndpoint");
                    return expectedResponse.StatusCode == HttpStatusCode.OK;
                },
                timeLimit: TimeSpan.FromMinutes(1),
                delay: TimeSpan.FromSeconds(2));

            // Assert
            var content = await expectedResponse!.Content.ReadAsStringAsync();
            content.Should().Contain("myExpectedContent");
        }

        // This is just to be able to verify everything works with regards to settings and executing the tests after deployment.
        // If needed, this test can be removed when the actual system test has been implemented.
        // Or it can be used to actually verify the HealthStatus after deplyoment.
        [SystemFact]
        public async Task When_RequestHealthStatus_Then_ResponseIsOKAndDoesNotContainFalse()
        {
            // Arrange
            using var httpClient = new HttpClient
            {
                BaseAddress = Configuration.BaseAddress,
            };

            // Act
            using var actualResponse = await httpClient.GetAsync("api/HealthStatus");

            // Assert
            actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await actualResponse.Content.ReadAsStringAsync();
            content.Should().NotContain("false");
        }
#pragma warning restore VSTHRD200 // Use "Async" suffix for async methods
    }
}
