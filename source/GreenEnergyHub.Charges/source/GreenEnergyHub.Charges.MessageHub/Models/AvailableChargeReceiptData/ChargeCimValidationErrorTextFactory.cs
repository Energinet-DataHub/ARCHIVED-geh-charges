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
using System.Linq;

using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.ValidationErrors;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;
using GreenEnergyHub.Charges.MessageHub.Models.Shared;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeReceiptData
{
    public class ChargeCimValidationErrorTextFactory : ICimValidationErrorTextFactory<ChargeInformationCommand, ChargeOperationDto>
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
            ChargeInformationCommand command,
            ChargeOperationDto chargeOperationDto)
        {
            return GetMergedErrorMessage(validationError, command, chargeOperationDto);
        }

        private string GetMergedErrorMessage(
            ValidationError validationError,
            ChargeInformationCommand chargeInformationCommand,
            ChargeOperationDto chargeOperationDto)
        {
            var errorTextTemplate = _cimValidationErrorTextProvider
                .GetCimValidationErrorText(validationError.ValidationRuleIdentifier);

            return MergeErrorText(
                errorTextTemplate, chargeInformationCommand, chargeOperationDto, validationError.TriggeredBy);
        }

        private string MergeErrorText(
            string errorTextTemplate,
            ChargeInformationCommand chargeInformationCommand,
            ChargeOperationDto chargeOperationDto,
            string? triggeredBy)
        {
            var tokens = CimValidationErrorTextTokenMatcher.GetTokens(errorTextTemplate);

            var mergedErrorText = errorTextTemplate;

            foreach (var token in tokens)
            {
                var data = GetDataForToken(token, chargeInformationCommand, chargeOperationDto, triggeredBy);
                mergedErrorText = mergedErrorText.Replace("{{" + token + "}}", data);
            }

            return mergedErrorText;
        }

        private string GetDataForToken(
            CimValidationErrorTextToken token,
            ChargeInformationCommand chargeInformationCommand,
            ChargeOperationDto chargeOperationDto,
            string? triggeredBy)
        {
            // Please keep sorted by CimValidationErrorTextToken
            return token switch
            {
                CimValidationErrorTextToken.ChargeDescription =>
                    chargeOperationDto.ChargeDescription,
                CimValidationErrorTextToken.ChargeName =>
                    chargeOperationDto.ChargeName,
                CimValidationErrorTextToken.ChargeOwner =>
                    chargeOperationDto.ChargeOwner,
                CimValidationErrorTextToken.ChargePointPosition =>
                    GetPosition(chargeOperationDto, triggeredBy),
                CimValidationErrorTextToken.ChargePointPrice =>
                    GetPriceFromPointByPosition(chargeOperationDto, triggeredBy),
                CimValidationErrorTextToken.ChargePointsCount =>
                    chargeOperationDto.Points.Count.ToString(),
                CimValidationErrorTextToken.ChargeResolution =>
                    chargeOperationDto.Resolution.ToString(),
                CimValidationErrorTextToken.ChargeStartDateTime =>
                    chargeOperationDto.StartDateTime.ToString(),
                CimValidationErrorTextToken.ChargeTaxIndicator =>
                    chargeOperationDto.TaxIndicator.ToString(),
                CimValidationErrorTextToken.ChargeType =>
                    chargeOperationDto.Type.ToString(),
                CimValidationErrorTextToken.ChargeVatClass =>
                    chargeOperationDto.VatClassification.ToString(),
                CimValidationErrorTextToken.DocumentBusinessReasonCode =>
                    chargeInformationCommand.Document.BusinessReasonCode.ToString(),
                CimValidationErrorTextToken.DocumentId =>
                    chargeInformationCommand.Document.Id,
                CimValidationErrorTextToken.DocumentSenderId =>
                    chargeInformationCommand.Document.Sender.MarketParticipantId,
                CimValidationErrorTextToken.DocumentSenderProvidedChargeId =>
                    chargeOperationDto.ChargeId,
                CimValidationErrorTextToken.DocumentType =>
                    chargeInformationCommand.Document.Type.ToString(),
                CimValidationErrorTextToken.TriggeredByOperationId =>
                    GetOperationIdFromTriggeredBy(triggeredBy),
                CimValidationErrorTextToken.ChargeOperationId =>
                    chargeOperationDto.Id,
                CimValidationErrorTextToken.DocumentRecipientBusinessProcessRole =>
                    chargeInformationCommand.Document.Recipient.BusinessProcessRole.ToString(),
                _ => CimValidationErrorTextTemplateMessages.Unknown,

            };
        }

        private string GetPosition(ChargeOperationDto chargeOperationDto,  string? triggeredBy)
        {
            var parsed = int.TryParse(triggeredBy, out var position);
            if (!string.IsNullOrWhiteSpace(triggeredBy) && parsed && position > 0)
                return triggeredBy;

            var errorMessage = $"Invalid position ({triggeredBy}) for charge with " +
                               $"id: {chargeOperationDto.ChargeId}," +
                               $"type: {chargeOperationDto.Type}," +
                               $"owner: {chargeOperationDto.ChargeOwner}";
            _logger.LogError("Invalid position: {errorMessage}", errorMessage);

            return CimValidationErrorTextTemplateMessages.Unknown;
        }

        private string GetPriceFromPointByPosition(ChargeOperationDto chargeOperationDto, string? triggeredBy)
        {
            try
            {
                return chargeOperationDto.Points
                        .Single(p => p.Position == int.Parse(triggeredBy!))
                        .Price.ToString("N");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Price not found {errorMessage}", $"by position: {triggeredBy}");

                return CimValidationErrorTextTemplateMessages.Unknown;
            }
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
