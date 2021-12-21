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
using GreenEnergyHub.Charges.Application.ChargeLinks.CreateDefaultChargeLinkReplier;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksDataAvailableNotifiedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.DefaultChargeLinksDataAvailableNotifiedEvents;
using GreenEnergyHub.Charges.Infrastructure.Core.MessageMetaData;

namespace GreenEnergyHub.Charges.Application.ChargeLinks.Handlers
{
    public class CreateDefaultChargeLinksReplyHandler : ICreateDefaultChargeLinksReplyHandler
    {
        private readonly IMessageMetaDataContext _messageMetaDataContext;
        private readonly ICreateDefaultChargeLinksReplier _createDefaultChargeLinksReplier;

        public CreateDefaultChargeLinksReplyHandler(
            IMessageMetaDataContext messageMetaDataContext,
            ICreateDefaultChargeLinksReplier createDefaultChargeLinksReplier)
        {
            _messageMetaDataContext = messageMetaDataContext;
            _createDefaultChargeLinksReplier = createDefaultChargeLinksReplier;
        }

        public async Task HandleAsync(ChargeLinksDataAvailableNotifiedEvent chargeLinksDataAvailableNotifiedEvent)
        {
            await _createDefaultChargeLinksReplier
                .ReplyWithSucceededAsync(
                    chargeLinksDataAvailableNotifiedEvent.MeteringPointId,
                    true,
                    _messageMetaDataContext.ReplyTo).ConfigureAwait(false);
        }
    }
}
