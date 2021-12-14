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
using Energinet.Charges.Contracts;
using Energinet.DataHub.Charges.Clients.DefaultChargeLink.Models;
using Energinet.DataHub.Charges.Clients.ServiceBus;
using Energinet.DataHub.Charges.Clients.ServiceBus.Providers;
using Google.Protobuf;

namespace Energinet.DataHub.Charges.Clients.DefaultChargeLink
{
    public sealed class DefaultChargeLinkClient : IDefaultChargeLinkClient
    {
        private readonly IServiceBusRequestSender _serviceBusRequestSender;

        public DefaultChargeLinkClient(
            [DisallowNull] IServiceBusRequestSenderProvider serviceBusRequestSenderProvider)
        {
            if (serviceBusRequestSenderProvider == null)
                throw new ArgumentNullException(nameof(serviceBusRequestSenderProvider));

            _serviceBusRequestSender = serviceBusRequestSenderProvider.GetInstance();
        }

        public async Task CreateDefaultChargeLinksRequestAsync(
            [DisallowNull] RequestDefaultChargeLinksForMeteringPointDto requestDefaultChargeLinksForMeteringPointDto,
            [DisallowNull] string correlationId)
        {
            if (requestDefaultChargeLinksForMeteringPointDto == null)
                throw new ArgumentNullException(nameof(requestDefaultChargeLinksForMeteringPointDto));

            if (string.IsNullOrWhiteSpace(correlationId))
                throw new ArgumentNullException(nameof(correlationId));

            var createDefaultChargeLinks = new CreateDefaultChargeLinks
            {
                MeteringPointId = requestDefaultChargeLinksForMeteringPointDto.MeteringPointId,
            };

            await _serviceBusRequestSender.SendRequestAsync(
                createDefaultChargeLinks.ToByteArray(), correlationId)
                .ConfigureAwait(false);
        }
    }
}
