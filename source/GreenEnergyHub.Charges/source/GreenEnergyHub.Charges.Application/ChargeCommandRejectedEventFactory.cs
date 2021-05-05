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
using System.Linq;
using GreenEnergyHub.Charges.Application.Validation;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using GreenEnergyHub.Charges.Domain.Events.Local;
using NodaTime;

namespace GreenEnergyHub.Charges.Application
{
    public class ChargeCommandRejectedEventFactory : IChargeCommandRejectedEventFactory
    {
        private readonly IClock _clock;

        public ChargeCommandRejectedEventFactory(IClock clock)
        {
            _clock = clock;
        }

        public IInternalEvent CreateEvent(
            [NotNull] ChargeCommand command,
            [NotNull] ChargeCommandValidationResult chargeCommandValidationResult)
        {
            return new ChargeCommandRejectedEvent(
                _clock.GetCurrentInstant(),
                command.ChargeEvent.CorrelationId,
                command.Document.Id,
                command.ChargeEvent.Id,
                chargeCommandValidationResult.InvalidRules.Select(x => x.ValidationRuleIdentifier.ToString()).ToArray());
        }

        public IInternalEvent CreateEvent(
            [NotNull] ChargeCommand command,
            [NotNull] Exception exception)
        {
            return new ChargeCommandRejectedEvent(
                _clock.GetCurrentInstant(),
                command.ChargeEvent.CorrelationId,
                command.Document.Id,
                command.ChargeEvent.Id,
                new[] { exception.Message });
        }
    }
}
