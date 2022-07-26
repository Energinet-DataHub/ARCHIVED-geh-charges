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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.Application.Charges.Services
{
    public class ChargePriceRejectionService : IChargePriceRejectionService
    {
        private readonly ILogger _logger;

        public ChargePriceRejectionService(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(nameof(ChargePriceConfirmationService));
        }

        public Task SaveRejectionsAsync(
            List<ChargePriceOperationDto> rejectedPriceOperations,
            ValidationResult documentValidationResult)
        {
            foreach (var chargePriceOperationDto in rejectedPriceOperations)
            {
                _logger.LogInformation(
                    $"{chargePriceOperationDto.ChargeId} rejected price operations was persisted. With errors: {PrintInvalidRules(documentValidationResult)}");
            }

            return Task.CompletedTask;
        }

        public Task SaveRejectionsAsync(
            List<ChargePriceOperationDto> operationsToBeRejected,
            List<IValidationRuleContainer> documentValidationResult)
        {
            foreach (var chargePriceOperationDto in operationsToBeRejected)
            {
                _logger.LogInformation(
                    $"{chargePriceOperationDto.ChargeId} rejected price operations was persisted. With errors: {PrintInvalidRules(ValidationResult.CreateFailure(documentValidationResult))}");
            }

            return Task.CompletedTask;
        }

        private static string PrintInvalidRules(ValidationResult documentValidationResult)
        {
            return string.Join(
                ",",
                documentValidationResult.InvalidRules.Select(x =>
                    x.ValidationRule.ValidationRuleIdentifier.ToString()));
        }
    }
}
