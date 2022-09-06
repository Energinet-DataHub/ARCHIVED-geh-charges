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
    public class ChargeLinksCimValidationErrorTextFactory : ICimValidationErrorTextFactory<ChargeLinksCommand, ChargeLinkOperationDto>
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
            ChargeLinkOperationDto chargeLinkOperationDto)
        {
            return GetMergedErrorMessage(validationError, chargeLinkOperationDto);
        }

        private string GetMergedErrorMessage(
            ValidationError validationError,
            ChargeLinkOperationDto chargeLinkOperationDto)
        {
            var errorTextTemplate = _cimValidationErrorTextProvider
                .GetCimValidationErrorText(validationError.ValidationRuleIdentifier);

            return MergeErrorText(errorTextTemplate, chargeLinkOperationDto, validationError.TriggeredBy);
        }

        private string MergeErrorText(
            string errorTextTemplate,
            ChargeLinkOperationDto chargeLinkOperationDto,
            string? triggeredBy)
        {
            var tokens = CimValidationErrorTextTokenMatcher.GetTokens(errorTextTemplate);

            var mergedErrorText = errorTextTemplate;

            foreach (var token in tokens)
            {
                var data = GetDataForToken(token, chargeLinkOperationDto, triggeredBy);
                mergedErrorText = mergedErrorText.Replace("{{" + token + "}}", data);
            }

            return mergedErrorText;
        }

        private string GetDataForToken(
            CimValidationErrorTextToken token,
            ChargeLinkOperationDto chargeLinkOperationDto,
            string? triggeredBy)
        {
            // Please keep sorted by CimValidationErrorTextToken
            return token switch
            {
                CimValidationErrorTextToken.ChargeLinkStartDate =>
                    chargeLinkOperationDto.StartDateTime.ToString(),
                CimValidationErrorTextToken.ChargeOperationId =>
                    chargeLinkOperationDto.OperationId,
                CimValidationErrorTextToken.ChargeOwner =>
                    chargeLinkOperationDto.ChargeOwner,
                CimValidationErrorTextToken.ChargeStartDateTime =>
                    chargeLinkOperationDto.StartDateTime.ToString(),
                CimValidationErrorTextToken.ChargeType =>
                    chargeLinkOperationDto.ChargeType.ToString(),
                CimValidationErrorTextToken.DocumentSenderProvidedChargeId =>
                    chargeLinkOperationDto.SenderProvidedChargeId,
                CimValidationErrorTextToken.MeteringPointId =>
                    chargeLinkOperationDto.MeteringPointId,
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
