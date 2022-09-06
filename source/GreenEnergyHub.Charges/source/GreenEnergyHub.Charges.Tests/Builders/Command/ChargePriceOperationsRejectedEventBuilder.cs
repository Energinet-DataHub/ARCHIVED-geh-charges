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
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using NodaTime;

namespace GreenEnergyHub.Charges.Tests.Builders.Command
{
    public class ChargePriceOperationsRejectedEventBuilder
    {
        private Instant _publishedTime;
        private ChargePriceCommand _chargePriceCommand;
        private IEnumerable<ValidationError> _validationErrors;

        public ChargePriceOperationsRejectedEventBuilder()
        {
            _publishedTime = SystemClock.Instance.GetCurrentInstant();
            _chargePriceCommand = new ChargePriceCommandBuilder().Build();
            _validationErrors = new List<ValidationError>();
        }

        public ChargePriceOperationsRejectedEventBuilder WithChargeCommand(ChargePriceCommand chargePriceCommand)
        {
            _chargePriceCommand = chargePriceCommand;
            return this;
        }

        public ChargePriceOperationsRejectedEventBuilder WithValidationErrors(IEnumerable<ValidationError> validationErrors)
        {
            _validationErrors = validationErrors;
            return this;
        }

        public ChargePriceOperationsRejectedEvent Build()
        {
            if (!_validationErrors.Any())
            {
                _validationErrors = new List<ValidationError>
                {
                    new ValidationError(
                        ValidationRuleIdentifier.MaximumPrice,
                        _chargePriceCommand.Operations.First().OperationId,
                        string.Empty),
                };
            }

            return new ChargePriceOperationsRejectedEvent(_publishedTime, _chargePriceCommand, _validationErrors);
        }
    }
}