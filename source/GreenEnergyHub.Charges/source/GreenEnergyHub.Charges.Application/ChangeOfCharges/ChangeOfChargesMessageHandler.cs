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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Transactions;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Message;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Result;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;

namespace GreenEnergyHub.Charges.Application.ChangeOfCharges
{
    public class ChangeOfChargesMessageHandler : IChangeOfChargesMessageHandler
    {
        private readonly IChangeOfChargesTransactionHandler _changeOfChargesTransactionHandler;

        public ChangeOfChargesMessageHandler(IChangeOfChargesTransactionHandler changeOfChargesTransactionHandler)
        {
            _changeOfChargesTransactionHandler = changeOfChargesTransactionHandler;
        }

        public async Task<ChangeOfChargesMessageResult> HandleAsync([NotNull] ChangeOfChargesMessage message)
        {
            var result = await HandleTransactionsAsync(message.Transactions).ConfigureAwait(false);
            return result;
        }

        private async Task<ChangeOfChargesMessageResult> HandleTransactionsAsync(List<ChangeOfChargesTransaction> transactions)
        {
            foreach (ChangeOfChargesTransaction transaction in transactions)
            {
                await _changeOfChargesTransactionHandler.HandleAsync(transaction).ConfigureAwait(false);
            }

            return ChangeOfChargesMessageResult.CreateSuccess();
        }
    }
}
