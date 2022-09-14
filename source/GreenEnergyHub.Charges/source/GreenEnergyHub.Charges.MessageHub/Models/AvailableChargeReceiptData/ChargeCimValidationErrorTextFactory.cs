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

using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.ValidationErrors;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;
using GreenEnergyHub.Charges.MessageHub.Models.Shared;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeReceiptData
{
    public class ChargeCimValidationErrorTextFactory : ICimValidationErrorTextFactory<ChargeInformationOperationDto>
    {
        private readonly ICimValidationErrorTextProvider _cimValidationErrorTextProvider;
        private readonly ILogger _logger;

        public ChargeCimValidationErrorTextFactory(
            ICimValidationErrorTextProvider cimValidationErrorTextProvider,
            ILoggerFactory loggerFactory)
        {
            _cimValidationErrorTextProvider = cimValidationErrorTextProvider;
            _logger = loggerFactory.CreateLogger(nameof(ChargeCimValidationErrorTextFactory));
        }

        public string Create(
            ValidationError validationError,
            DocumentDto document,
            ChargeInformationOperationDto chargeInformationOperationDto)
        {
            return GetMergedErrorMessage(validationError, document, chargeInformationOperationDto);
        }

        private string GetMergedErrorMessage(
            ValidationError validationError,
            DocumentDto document,
            ChargeInformationOperationDto chargeInformationOperationDto)
        {
            var errorTextTemplate = _cimValidationErrorTextProvider
                .GetCimValidationErrorText(validationError.ValidationRuleIdentifier);

            return MergeErrorText(
                errorTextTemplate, document, chargeInformationOperationDto, validationError.TriggeredBy);
        }

        private string MergeErrorText(
            string errorTextTemplate,
            DocumentDto document,
            ChargeInformationOperationDto chargeInformationOperationDto,
            string? triggeredBy)
        {
            var tokens = CimValidationErrorTextTokenMatcher.GetTokens(errorTextTemplate);

            var mergedErrorText = errorTextTemplate;

            foreach (var token in tokens)
            {
                var data = GetDataForToken(token, document, chargeInformationOperationDto, triggeredBy);
                mergedErrorText = mergedErrorText.Replace("{{" + token + "}}", data);
            }

            return mergedErrorText;
        }

        private string GetDataForToken(
            CimValidationErrorTextToken token,
            DocumentDto document,
            ChargeInformationOperationDto chargeInformationOperationDto,
            string? triggeredBy)
        {
            // Please keep sorted by CimValidationErrorTextToken
            return token switch
            {
                CimValidationErrorTextToken.ChargeDescription =>
                    chargeInformationOperationDto.ChargeDescription,
                CimValidationErrorTextToken.ChargeName =>
                    chargeInformationOperationDto.ChargeName,
                CimValidationErrorTextToken.ChargeOwner =>
                    chargeInformationOperationDto.ChargeOwner,
                CimValidationErrorTextToken.ChargePointPosition =>
                    GetPosition(chargeInformationOperationDto, triggeredBy),
                CimValidationErrorTextToken.ChargeResolution =>
                    chargeInformationOperationDto.Resolution.ToString(),
                CimValidationErrorTextToken.ChargeStartDateTime =>
                    chargeInformationOperationDto.StartDateTime.ToString(),
                CimValidationErrorTextToken.ChargeTaxIndicator =>
                    chargeInformationOperationDto.TaxIndicator.ToString(),
                CimValidationErrorTextToken.ChargeType =>
                    chargeInformationOperationDto.ChargeType.ToString(),
                CimValidationErrorTextToken.ChargeVatClass =>
                    chargeInformationOperationDto.VatClassification.ToString(),
                CimValidationErrorTextToken.DocumentBusinessReasonCode =>
                    BusinessReasonCode.UpdateChargeInformation.ToString(),
                CimValidationErrorTextToken.DocumentId =>
                    document.Id,
                CimValidationErrorTextToken.DocumentSenderId =>
                    document.Sender.MarketParticipantId,
                CimValidationErrorTextToken.DocumentSenderProvidedChargeId =>
                    chargeInformationOperationDto.SenderProvidedChargeId,
                CimValidationErrorTextToken.DocumentType =>
                    document.Type.ToString(),
                CimValidationErrorTextToken.TriggeredByOperationId =>
                    GetOperationIdFromTriggeredBy(triggeredBy),
                CimValidationErrorTextToken.ChargeOperationId =>
                    chargeInformationOperationDto.OperationId,
                CimValidationErrorTextToken.DocumentRecipientBusinessProcessRole =>
                    document.Recipient.BusinessProcessRole.ToString(),
                _ => CimValidationErrorTextTemplateMessages.Unknown,

            };
        }

        private string GetPosition(ChargeInformationOperationDto chargeInformationOperationDto,  string? triggeredBy)
        {
            var parsed = int.TryParse(triggeredBy, out var position);
            if (!string.IsNullOrWhiteSpace(triggeredBy) && parsed && position > 0)
                return triggeredBy;

            var errorMessage = $"Invalid position ({triggeredBy}) for charge with " +
                               $"id: {chargeInformationOperationDto.SenderProvidedChargeId}," +
                               $"type: {chargeInformationOperationDto.ChargeType}," +
                               $"owner: {chargeInformationOperationDto.ChargeOwner}";
            _logger.LogError("Invalid position: {ErrorMessage}", errorMessage);

            return CimValidationErrorTextTemplateMessages.Unknown;
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
