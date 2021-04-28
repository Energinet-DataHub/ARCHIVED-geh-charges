﻿// Copyright 2020 Energinet DataHub A/S
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
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using GreenEnergyHub.Charges.Domain.Events.Local;
using NodaTime;

namespace GreenEnergyHub.Charges.Application
{
    public class ChargeCommandAcceptedEventFactory : IChargeCommandAcceptedEventFactory
    {
        private readonly IClock _clock;

        public ChargeCommandAcceptedEventFactory(IClock clock)
        {
            _clock = clock;
        }

        public ChargeCommandAcceptedEvent CreateEvent([NotNull] ChargeCommand command)
        {
            return new ChargeCommandAcceptedEvent(
                _clock.GetCurrentInstant(),
                command.CorrelationId!,
                command);
        }
    }
}
