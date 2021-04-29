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

using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using GreenEnergyHub.Messaging.Transport;

namespace GreenEnergyHub.Charges.Infrastructure.Messaging
{
    public class ServiceBusChannel : Channel
    {
        private ServiceBusSender _serviceBusSender;
        private ICorrelationContext _correlationContext;

        public ServiceBusChannel(ServiceBusSender serviceBusSender, ICorrelationContext correlationContext)
        {
            _serviceBusSender = serviceBusSender;
            _correlationContext = correlationContext;
        }

        /// <summary>
        /// Write the <paramref name="data"/> to the channel
        /// </summary>
        /// <param name="data">data to write</param>
        /// <param name="cancellationToken">cancellation token</param>
        protected override Task WriteAsync(byte[] data, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Task.CompletedTask);
        }
    }
}
