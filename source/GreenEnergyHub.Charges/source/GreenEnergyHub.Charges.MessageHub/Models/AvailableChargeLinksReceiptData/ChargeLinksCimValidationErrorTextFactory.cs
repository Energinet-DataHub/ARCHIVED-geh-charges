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

using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.ValidationErrors;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;
using GreenEnergyHub.Charges.MessageHub.Models.Shared;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeLinksReceiptData
{
    public class ChargeLinksCimValidationErrorTextFactory : ICimValidationErrorTextFactory<ChargeLinksCommand, ChargeLinkDto>
    {
        private readonly ICimValidationErrorTextProvider _cimValidationErrorTextProvider;
        private readonly ILogger _logger;

        public ChargeLinksCimValidationErrorTextFactory(
            ICimValidationErrorTextProvider cimValidationErrorTextProvider,
            ILoggerFactory loggerFactory)
        {
            _cimValidationErrorTextProvider = cimValidationErrorTextProvider;
            _logger = loggerFactory.CreateLogger(nameof(ChargeLinksCimValidationErrorTextFactory));
        }

        public string Create(
            ValidationError validationError,
            ChargeLinksCommand command,
            ChargeLinkDto chargeLinkDto)
        {
            return GetMergedErrorMessage(validationError, chargeLinkDto);
        }

        private string GetMergedErrorMessage(
            ValidationError validationError,
            ChargeLinkDto chargeLinkDto)
        {
            var errorTextTemplate = _cimValidationErrorTextProvider
                .GetCimValidationErrorText(validationError.ValidationRuleIdentifier);

            return MergeErrorText(errorTextTemplate, chargeLinkDto, validationError.TriggeredBy);
        }

        private string MergeErrorText(
            string errorTextTemplate,
            ChargeLinkDto chargeLinkDto,
            string? triggeredBy)
        {
            var tokens = CimValidationErrorTextTokenMatcher.GetTokens(errorTextTemplate);

            var mergedErrorText = errorTextTemplate;

            foreach (var token in tokens)
            {
                var data = GetDataForToken(token, chargeLinkDto, triggeredBy);
                mergedErrorText = mergedErrorText.Replace("{{" + token + "}}", data);
            }

            return mergedErrorText;
        }

        private string GetDataForToken(
            CimValidationErrorTextToken token,
            ChargeLinkDto chargeLinkDto,
            string? triggeredBy)
        {
            // Please keep sorted by CimValidationErrorTextToken
            return token switch
            {
                CimValidationErrorTextToken.ChargeLinkStartDate =>
                    chargeLinkDto.StartDateTime.ToString(),
                CimValidationErrorTextToken.ChargeOperationId =>
                    chargeLinkDto.OperationId,
                CimValidationErrorTextToken.ChargeOwner =>
                    chargeLinkDto.ChargeOwner,
                CimValidationErrorTextToken.ChargeStartDateTime =>
                    chargeLinkDto.StartDateTime.ToString(),
                CimValidationErrorTextToken.ChargeType =>
                    chargeLinkDto.ChargeType.ToString(),
                CimValidationErrorTextToken.DocumentSenderProvidedChargeId =>
                    chargeLinkDto.SenderProvidedChargeId,
                CimValidationErrorTextToken.MeteringPointId =>
                    chargeLinkDto.MeteringPointId,
                CimValidationErrorTextToken.TriggeredByOperationId =>
                    GetOperationIdFromTriggeredBy(triggeredBy),
                _ => CimValidationErrorTextTemplateMessages.Unknown,
            };
        }

        private string GetOperationIdFromTriggeredBy(string? triggeredBy)
        {
            if (!string.IsNullOrWhiteSpace(triggeredBy))
                return triggeredBy;

            _logger.LogError("Id for failed operation is null");

            return CimValidationErrorTextTemplateMessages.Unknown;
        }
    }
}
