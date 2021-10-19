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
using Energinet.DataHub.MessageHub.Client.DataAvailable;
using Energinet.DataHub.MessageHub.Client.Model;
using Energinet.DataHub.MessageHub.Client.Peek;
using Microsoft.Azure.Functions.Worker;

namespace GreenEnergyHub.Charges.TestFunctionHost.MessageHub
{
    public class MessageHubBundleRequesterEndpoint
    {
        private const string FunctionName = nameof(MessageHubBundleRequesterEndpoint);
        private readonly IDataBundleRequestSender _dataBundleRequestSender;

        public MessageHubBundleRequesterEndpoint(IDataBundleRequestSender dataBundleRequestSender)
        {
            _dataBundleRequestSender = dataBundleRequestSender;
        }

        [Function(FunctionName)]
        public async Task RunAsync(
            [ServiceBusTrigger("sbq-charges", Connection = "INTEGRATIONEVENT_LISTENER_CONNECTION_STRING")]
            byte[] data)
        {
            var notification = new DataAvailableNotificationParser().Parse(data);
            await RequestBundleFromNotificationAsync(notification);
        }

        private async Task RequestBundleFromNotificationAsync(DataAvailableNotificationDto notification)
        {
            var idempotencyId = Guid.NewGuid().ToString();
            var dataAvailableNotificationIds = new[] { notification.Uuid };
            var request = new DataBundleRequestDto(idempotencyId, dataAvailableNotificationIds);

            await _dataBundleRequestSender.SendAsync(request, DomainOrigin.Charges);
        }
    }
}
