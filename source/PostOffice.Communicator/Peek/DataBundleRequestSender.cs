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
    public sealed class DataBundleRequestSender : IDataBundleRequestSender, IAsyncDisposable
    {
        private readonly IRequestBundleParser _requestBundleParser;
        private readonly IResponseBundleParser _responseBundleParser;
        private readonly IServiceBusClientFactory _serviceBusClientFactory;
        private readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(30);
        private ServiceBusClient? _serviceBusClient;

        public DataBundleRequestSender(
            IRequestBundleParser requestBundleParser,
            IResponseBundleParser responseBundleParser,
            IServiceBusClientFactory serviceBusClientFactory)
        {
            _requestBundleParser = requestBundleParser;
            _responseBundleParser = responseBundleParser;
            _serviceBusClientFactory = serviceBusClientFactory;
        }

        public async ValueTask DisposeAsync()
        {
            if (_serviceBusClient != null)
            {
                await _serviceBusClient.DisposeAsync().ConfigureAwait(false);
                _serviceBusClient = null;
            }
        }

        public async Task<RequestDataBundleResponseDto?> SendAsync(
            DataBundleRequestDto dataBundleRequestDto,
            DomainOrigin domainOrigin)
        {
            if (dataBundleRequestDto == null)
                throw new ArgumentNullException(nameof(dataBundleRequestDto));
            var bytes = _requestBundleParser.Parse(dataBundleRequestDto);

            var sessionId = Guid.NewGuid().ToString();
            var serviceBusMessage = new ServiceBusMessage(bytes)
            {
                SessionId = sessionId,
                ReplyToSessionId = sessionId,
                ReplyTo = $"sbq-{domainOrigin}-reply",
            };

            _serviceBusClient ??= _serviceBusClientFactory.Create();

            await using var sender = _serviceBusClient.CreateSender($"sbq-{domainOrigin}");
            await sender.SendMessageAsync(serviceBusMessage).ConfigureAwait(false);

            await using var receiver = await _serviceBusClient
                .AcceptSessionAsync($"sbq-{domainOrigin}-reply", sessionId)
                .ConfigureAwait(false);

            var response = await receiver.ReceiveMessageAsync(_defaultTimeout).ConfigureAwait(false);
            if (response == null)
                return null;
            var dataBundleResponseDto = _responseBundleParser.Parse(response.Body.ToArray());
            return dataBundleResponseDto;
        }
    }
}
