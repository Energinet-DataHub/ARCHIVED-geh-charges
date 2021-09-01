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
using GreenEnergyHub.Charges.Domain.Charges.Acknowledgements;
using GreenEnergyHub.Charges.Domain.Charges.Events.Local;

namespace GreenEnergyHub.Charges.Application.Charges.Acknowledgement
{
    public class ChargeRejectionSender : IChargeRejectionSender
    {
        private readonly IMessageDispatcher<ChargeRejection> _messageDispatcher;

        public ChargeRejectionSender(IMessageDispatcher<ChargeRejection> messageDispatcher)
        {
            _messageDispatcher = messageDispatcher;
        }

        public async Task HandleAsync([NotNull] ChargeCommandRejectedEvent rejectedEvent)
        {
            var chargeRejection = new ChargeRejection(
                rejectedEvent.CorrelationId,
                rejectedEvent.Command.Document.Sender.Id,
                rejectedEvent.Command.Document.Sender.BusinessProcessRole,
                rejectedEvent.Command.ChargeOperation.Id,
                rejectedEvent.Command.Document.BusinessReasonCode,
                rejectedEvent.RejectReasons);

            await _messageDispatcher.DispatchAsync(chargeRejection).ConfigureAwait(false);
        }
    }
}
