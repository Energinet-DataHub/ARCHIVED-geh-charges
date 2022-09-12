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

using System.Collections.Generic;
using System.Linq;
using GreenEnergyHub.Charges.Application.Charges.Events;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using NodaTime;

namespace GreenEnergyHub.Charges.Application.Charges.Factories
{
    public class PriceRejectedEventFactory : IPriceRejectedEventFactory
    {
        private readonly IClock _clock;

        public PriceRejectedEventFactory(IClock clock)
        {
            _clock = clock;
        }

        public PriceRejectedEvent Create(
            DocumentDto document,
            IReadOnlyCollection<ChargePriceOperationDto> operations,
            ValidationResult validationResult)
        {
            var validationErrors = validationResult.InvalidRules
                .Select(ValidationErrorFactory.Create());
            return new PriceRejectedEvent(
                _clock.GetCurrentInstant(),
                document,
                operations,
                validationErrors);
        }
    }
}
