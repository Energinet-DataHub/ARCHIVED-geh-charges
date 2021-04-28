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
using GreenEnergyHub.Charges.Application.ChangeOfCharges;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;

namespace GreenEnergyHub.Charges.Application
{
    public class ChargeCommandExecutionExceptionHandler : IChargeCommandExecutionExceptionHandler
    {
        private readonly IChargeCommandRejectedEventFactory _chargeCommandRejectedEventFactory;
        private readonly IInternalEventPublisher _internalEventPublisher;

        public ChargeCommandExecutionExceptionHandler(
            IChargeCommandRejectedEventFactory chargeCommandRejectedEventFactory,
            IInternalEventPublisher internalEventPublisher)
        {
            _chargeCommandRejectedEventFactory = chargeCommandRejectedEventFactory;
            _internalEventPublisher = internalEventPublisher;
        }

        public async Task ExecuteChargeCommandAsync(
            [NotNull] Func<Task> func,
            [NotNull] ChargeCommand command)
        {
            try
            {
                await func().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                var internalEvent = _chargeCommandRejectedEventFactory.CreateEvent(
                    command,
                    e);
                await _internalEventPublisher.PublishAsync(internalEvent).ConfigureAwait(false);
                throw;
            }
        }

        public async Task<TResult> ExecuteChargeCommandAsync<TResult>(
            [NotNull] Func<Task<TResult>> func,
            [NotNull] ChargeCommand command)
        {
            try
            {
                return await func().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                await PublishExceptionAsCommandRejectedEventAsync(command, e).ConfigureAwait(false);
                throw;
            }
        }

        private async Task PublishExceptionAsCommandRejectedEventAsync(ChargeCommand command, Exception e)
        {
            var internalEvent = _chargeCommandRejectedEventFactory.CreateEvent(
                command,
                e);
            await _internalEventPublisher.PublishAsync(internalEvent).ConfigureAwait(false);
        }
    }
}
