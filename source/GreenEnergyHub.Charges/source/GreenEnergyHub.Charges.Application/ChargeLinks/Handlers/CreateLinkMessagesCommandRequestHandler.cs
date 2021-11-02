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

using System.Threading.Tasks;
using Energinet.DataHub.Charges.Libraries.DefaultChargeLinkMessages;
using Energinet.DataHub.Charges.Libraries.Models;
using GreenEnergyHub.Charges.Domain.CreateLinkMessagesCommandEvent;

namespace GreenEnergyHub.Charges.Application.ChargeLinks.Handlers
{
    public class CreateLinkMessagesCommandRequestHandler : ICreateLinkMessagesCommandRequestHandler
    {
        private readonly IDefaultChargeLinkMessagesRequestClient _messagesRequestClient;
        private readonly IMessageMetaDataContext _messageMetaDataContext;

        public CreateLinkMessagesCommandRequestHandler(
            IDefaultChargeLinkMessagesRequestClient messagesRequestClient,
            IMessageMetaDataContext messageMetaDataContext)
        {
            _messagesRequestClient = messagesRequestClient;
            _messageMetaDataContext = messageMetaDataContext;
        }

        public async Task HandleAsync(CreateLinkMessagesCommandEvent createLinkCommandEvent, string correlationId)
        {
            // This is a stub implementation.
            await _messagesRequestClient
                .CreateDefaultChargeLinkMessagesSucceededRequestAsync(
                    new CreateDefaultChargeLinkMessagesSucceededDto(createLinkCommandEvent.MeteringPointId),
                    correlationId,
                    _messageMetaDataContext.ReplyTo)
                .ConfigureAwait(false);
        }
    }
}
