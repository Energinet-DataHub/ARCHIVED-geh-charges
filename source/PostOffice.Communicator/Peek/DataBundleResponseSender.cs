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
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using GreenEnergyHub.PostOffice.Communicator.Factories;
using GreenEnergyHub.PostOffice.Communicator.Model;

namespace GreenEnergyHub.PostOffice.Communicator.Peek
{
    public sealed class DataBundleResponseSender : IDataBundleResponseSender, IAsyncDisposable
    {
        private readonly IResponseBundleParser _responseBundleParser;
        private readonly IServiceBusClientFactory _serviceBusClientFactory;
        private readonly string _replyQueue;
        private ServiceBusClient? _serviceBusClient;

        public DataBundleResponseSender(
            IResponseBundleParser responseBundleParser,
            IServiceBusClientFactory serviceBusClientFactory,
            DomainOrigin domainOrigin)
        {
            _responseBundleParser = responseBundleParser;
            _serviceBusClientFactory = serviceBusClientFactory;
            _replyQueue = $"sbq-{domainOrigin}-reply";
        }

        public async Task SendAsync(RequestDataBundleResponseDto requestDataBundleResponseDto, string sessionId)
        {
            if (requestDataBundleResponseDto == null)
                throw new ArgumentNullException(nameof(requestDataBundleResponseDto));

            if (sessionId == null)
                throw new ArgumentNullException(nameof(sessionId));

            var contractBytes = _responseBundleParser.Parse(requestDataBundleResponseDto);
            var serviceBusReplyMessage = new ServiceBusMessage(contractBytes)
            {
                SessionId = sessionId,
            };

            _serviceBusClient ??= _serviceBusClientFactory.Create();

            await using var sender = _serviceBusClient.CreateSender(_replyQueue);
            await sender.SendMessageAsync(serviceBusReplyMessage).ConfigureAwait(false);
        }

        public async ValueTask DisposeAsync()
        {
            if (_serviceBusClient != null)
            {
                await _serviceBusClient.DisposeAsync().ConfigureAwait(false);
                _serviceBusClient = null;
            }
        }
    }
}
