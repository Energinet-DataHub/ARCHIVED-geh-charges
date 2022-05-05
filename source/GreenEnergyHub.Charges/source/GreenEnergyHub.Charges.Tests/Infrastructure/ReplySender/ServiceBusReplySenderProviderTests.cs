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

using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using FluentAssertions;
using GreenEnergyHub.Charges.Infrastructure.ReplySender;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.ReplySender
{
    [UnitTest]
    public class ServiceBusReplySenderProviderTests
    {
        private const string ConnectionString =
        "Endpoint=sb://sbn-charges-fake.servicebus.windows.net/;SharedAccessKeyName=sbnar-fake-sender;SharedAccessKey=fakeAccessKey";

        [Fact]
        public async Task GetInstance_WhenStressed_AlwaysReturnsInstance()
        {
            // Arrange
            var serviceBusClient = new ServiceBusClient(ConnectionString);
            var sut = new ServiceBusReplySenderProvider(serviceBusClient);

            // Act & Assert
            var taskList = new List<Task>();
            for (var i = 0; i < 10000; i++)
            {
                var lastTask = new Task(() =>
                {
                    IServiceBusReplySender result = null!;
                    var exception = Record.Exception(() => result = sut.GetInstance("replyToTest"));

                    result.Should().NotBeNull();
                    exception.Should().BeNull();
                });
                lastTask.Start();
                taskList.Add(lastTask);
            }

            await Task.WhenAll(taskList.ToArray());
        }
    }
}
