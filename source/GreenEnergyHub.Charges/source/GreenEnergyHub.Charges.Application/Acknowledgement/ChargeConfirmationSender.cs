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

namespace GreenEnergyHub.Charges.Application.Acknowledgement
{
    public class ChargeConfirmationSender : IChargeConfirmationSender
    {
        private readonly IMessageDispatcher<ChargeConfirmation> _messageDispatcher;

        public ChargeConfirmationSender(IMessageDispatcher<ChargeConfirmation> messageDispatcher)
        {
            _messageDispatcher = messageDispatcher;
        }

        public async Task HandleAsync([NotNull] ChargeCommandAcceptedEvent acceptedEvent)
        {
            var confirmation = new ChargeConfirmation(
                acceptedEvent.CorrelationId,
                acceptedEvent.Command.Document.Sender.Id,
                acceptedEvent.Command.Document.Sender.BusinessProcessRole,
                acceptedEvent.Command.ChargeOperation.Id,
                acceptedEvent.Command.Document.BusinessReasonCode);

            await _messageDispatcher.DispatchAsync(confirmation).ConfigureAwait(false);
        }
    }
}
