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

using System.Linq;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using NodaTime;

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargePriceRejectedEvents
{
    public class ChargePriceRejectedEventFactory : IChargePriceRejectedEventFactory
    {
        private readonly IClock _clock;

        public ChargePriceRejectedEventFactory(IClock clock)
        {
            _clock = clock;
        }

        public ChargePriceRejectedEvent CreateEvent(ChargePriceCommand command, ValidationResult validationResult)
        {
            return new ChargePriceRejectedEvent(
                _clock.GetCurrentInstant(),
                command,
                validationResult.InvalidRules.Select(ValidationErrorFactory.Create()));
        }
    }
}
