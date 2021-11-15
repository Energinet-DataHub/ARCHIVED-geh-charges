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
using GreenEnergyHub.Charges.Application.Charges.Handlers.Message;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;

namespace GreenEnergyHub.Charges.Application.Charges.Handlers
{
    public class ChargesMessageHandler : IChargesMessageHandler
    {
        private readonly IChargeCommandHandler _chargeCommandHandler;

        public ChargesMessageHandler(IChargeCommandHandler chargeCommandHandler)
        {
            _chargeCommandHandler = chargeCommandHandler;
        }

        public async Task<ChargesMessageResult> HandleAsync([NotNull] ChargesMessage message)
        {
            var result = await HandleTransactionsAsync(message).ConfigureAwait(false);
            return result;
        }

        private async Task<ChargesMessageResult> HandleTransactionsAsync([NotNull] ChargesMessage message)
        {
            foreach (ChargeCommand transaction in message.Transactions)
            {
                await _chargeCommandHandler.HandleAsync(transaction).ConfigureAwait(false);
            }

            return ChargesMessageResult.CreateSuccess();
        }
    }
}
