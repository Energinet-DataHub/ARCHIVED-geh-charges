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
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace GreenEnergyHub.FunctionApp.TestCommon.ServiceBus.ListenerMock
{
    public static class DoProviderExtensions
    {
        public static async Task<CountdownEvent> VerifyCountAsync(this DoProvider provider, int expectedCount, Func<ServiceBusReceivedMessage, Task> messageHandler)
        {
            var whenAllReceivedEvent = new CountdownEvent(expectedCount);

            await provider.DoAsync(async (message) =>
            {
                await messageHandler(message).ConfigureAwait(false);
                whenAllReceivedEvent.Signal();
            }).ConfigureAwait(false);

            return whenAllReceivedEvent;
        }

        public static Task<CountdownEvent> VerifyCountAsync(this DoProvider provider, int expectedCount)
        {
            return VerifyCountAsync(provider, expectedCount, _ => Task.CompletedTask);
        }

        public static async Task<ManualResetEventSlim> VerifyOnceAsync(this DoProvider provider, Func<ServiceBusReceivedMessage, Task> messageHandler)
        {
            var whenOnceEvent = new ManualResetEventSlim(false);

            await provider.DoAsync(async (message) =>
            {
                await messageHandler(message).ConfigureAwait(false);
                whenOnceEvent.Set();
            }).ConfigureAwait(false);

            return whenOnceEvent;
        }

        public static Task<ManualResetEventSlim> VerifyOnceAsync(this DoProvider provider)
        {
            return VerifyOnceAsync(provider, _ => Task.CompletedTask);
        }
    }
}
