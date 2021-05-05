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

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Application;
using GreenEnergyHub.Charges.Domain;
using GreenEnergyHub.Charges.Domain.Events.Local;
using GreenEnergyHub.Charges.Infrastructure.PostOffice;

namespace GreenEnergyHub.Charges.Infrastructure
{
    // TODO: Move class to application project? Which would also bring IPostOfficeService to application
    public class ChargeAcknowledgementSender : IChargeAcknowledgementSender
    {
        private readonly IPostOfficeService _postOfficeService;

        public ChargeAcknowledgementSender(IPostOfficeService postOfficeService)
        {
            _postOfficeService = postOfficeService;
        }

        public async Task HandleAsync([NotNull] ChargeCommandAcceptedEvent acceptedEvent)
        {
            // TODO: Param: MktActivityRecord.OriginalTransactionReference_MktActivityRecord.mRID
            // TODO: Delegate construction to factory (but not in infrastructure project)
            var chargeCommandAcceptedAcknowledgement = new ChargeAcknowledgement(
                acceptedEvent.CorrelationId,
                acceptedEvent.Command.MarketDocument.ReceiverMarketParticipant.MRid,
                acceptedEvent.Command.MarketDocument.ReceiverMarketParticipant.Role,
                acceptedEvent.Command.MktActivityRecord.MRid,
                "MktActivityRecord.OriginalTransactionReference_MktActivityRecord.mRID",
                acceptedEvent.Command.MarketDocument.ProcessType);

            await _postOfficeService.SendAsync(chargeCommandAcceptedAcknowledgement).ConfigureAwait(false);
        }
    }
}
