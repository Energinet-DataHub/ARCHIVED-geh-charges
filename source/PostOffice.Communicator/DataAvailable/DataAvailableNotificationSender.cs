// // Copyright 2020 Energinet DataHub A/S
// //
// // Licensed under the Apache License, Version 2.0 (the "License2");
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //
// //     http://www.apache.org/licenses/LICENSE-2.0
// //
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.

using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Google.Protobuf;
using GreenEnergyHub.PostOffice.Communicator.Contracts;
using GreenEnergyHub.PostOffice.Communicator.Factories;
using GreenEnergyHub.PostOffice.Communicator.Model;

namespace GreenEnergyHub.PostOffice.Communicator.DataAvailable
{
    public sealed class DataAvailableNotificationSender : IDataAvailableNotificationSender, IAsyncDisposable
    {
        private readonly IServiceBusClientFactory _serviceBusClientFactory;
        private ServiceBusClient? _serviceBusClient;

        public DataAvailableNotificationSender(IServiceBusClientFactory serviceBusClientFactory)
        {
            _serviceBusClientFactory = serviceBusClientFactory;
        }

        public async Task SendAsync(DataAvailableNotificationDto dataAvailableNotificationDto)
        {
            if (dataAvailableNotificationDto == null)
                throw new ArgumentNullException(nameof(dataAvailableNotificationDto));

            _serviceBusClient ??= _serviceBusClientFactory.Create();

            await using var sender = _serviceBusClient.CreateSender("sbq-dataavailable");

            var contract = new DataAvailableNotificationContract
            {
                UUID = dataAvailableNotificationDto.Uuid.ToString(),
                MessageType = dataAvailableNotificationDto.MessageType.Value,
                Origin = dataAvailableNotificationDto.Origin.ToString(),
                Recipient = dataAvailableNotificationDto.Recipient.Value,
                SupportsBundling = dataAvailableNotificationDto.SupportsBundling,
                RelativeWeight = dataAvailableNotificationDto.RelativeWeight,
            };

            var message = new ServiceBusMessage(new BinaryData(contract.ToByteArray()));
            await sender.SendMessageAsync(message).ConfigureAwait(false);
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
