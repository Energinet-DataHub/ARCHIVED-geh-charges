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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.ValidationErrors;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;
using GreenEnergyHub.Charges.MessageHub.Models.Shared;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeReceiptData
{
    public class ChargeInformationCimValidationErrorTextFactory : ICimValidationErrorTextFactory<ChargeCommand, ChargeInformationDto>
    {
        private readonly ICimValidationErrorTextProvider _cimValidationErrorTextProvider;
        private readonly ILogger _logger;

        public ChargeInformationCimValidationErrorTextFactory(
            ICimValidationErrorTextProvider cimValidationErrorTextProvider,
            ILoggerFactory loggerFactory)
        {
            _cimValidationErrorTextProvider = cimValidationErrorTextProvider;
            _logger = loggerFactory.CreateLogger(nameof(ChargeInformationCimValidationErrorTextFactory));
        }

        public string Create(
            ValidationError validationError,
            ChargeCommand command,
            ChargeInformationDto chargeInformationDto)
        {
            return GetMergedErrorMessage(validationError, command, chargeInformationDto);
        }

        private string GetMergedErrorMessage(
            ValidationError validationError,
            ChargeCommand chargeCommand,
            ChargeInformationDto chargeInformationDto)
        {
            var errorTextTemplate = _cimValidationErrorTextProvider
                .GetCimValidationErrorText(validationError.ValidationRuleIdentifier);

            return MergeErrorText(errorTextTemplate, chargeCommand, chargeInformationDto, validationError.TriggeredBy);
        }

        private string MergeErrorText(
            string errorTextTemplate,
            ChargeCommand chargeCommand,
            ChargeInformationDto chargeInformationDto,
            string? triggeredBy)
        {
            var tokens = CimValidationErrorTextTokenMatcher.GetTokens(errorTextTemplate);

            var mergedErrorText = errorTextTemplate;

            foreach (var token in tokens)
            {
                var data = GetDataForToken(token, chargeCommand, chargeInformationDto, triggeredBy);
                mergedErrorText = mergedErrorText.Replace("{{" + token + "}}", data);
            }

            return mergedErrorText;
        }

        private string GetDataForToken(
            CimValidationErrorTextToken token,
            ChargeCommand chargeCommand,
            ChargeInformationDto chargeInformationDto,
            string? triggeredBy)
        {
            // Please keep sorted by CimValidationErrorTextToken
            return token switch
            {
                CimValidationErrorTextToken.ChargeDescription =>
                    chargeInformationDto.ChargeDescription,
                CimValidationErrorTextToken.ChargeName =>
                    chargeInformationDto.ChargeName,
                CimValidationErrorTextToken.ChargeOwner =>
                    chargeInformationDto.ChargeOwner,
                CimValidationErrorTextToken.ChargePointPosition =>
                    GetPosition(chargeInformationDto, triggeredBy),
                CimValidationErrorTextToken.ChargePointPrice =>
                    GetPriceFromPointByPosition(chargeInformationDto, triggeredBy),
                CimValidationErrorTextToken.ChargePointsCount =>
                    chargeInformationDto.Points.Count.ToString(),
                CimValidationErrorTextToken.ChargeResolution =>
                    chargeInformationDto.Resolution.ToString(),
                CimValidationErrorTextToken.ChargeStartDateTime =>
                    chargeInformationDto.StartDateTime.ToString(),
                CimValidationErrorTextToken.ChargeTaxIndicator =>
                    chargeInformationDto.TaxIndicator.ToString(),
                CimValidationErrorTextToken.ChargeType =>
                    chargeInformationDto.Type.ToString(),
                CimValidationErrorTextToken.ChargeVatClass =>
                    chargeInformationDto.VatClassification.ToString(),
                CimValidationErrorTextToken.DocumentBusinessReasonCode =>
                    chargeCommand.Document.BusinessReasonCode.ToString(),
                CimValidationErrorTextToken.DocumentId =>
                    chargeCommand.Document.Id,
                CimValidationErrorTextToken.DocumentSenderId =>
                    chargeCommand.Document.Sender.Id,
                CimValidationErrorTextToken.DocumentSenderProvidedChargeId =>
                    chargeInformationDto.ChargeId,
                CimValidationErrorTextToken.DocumentType =>
                    chargeCommand.Document.Type.ToString(),
                CimValidationErrorTextToken.TriggeredByOperationId =>
                    GetOperationIdFromTriggeredBy(triggeredBy),
                CimValidationErrorTextToken.ChargeOperationId =>
                    chargeInformationDto.Id,
                CimValidationErrorTextToken.DocumentRecipientBusinessProcessRole =>
                    chargeCommand.Document.Recipient.BusinessProcessRole.ToString(),
                _ => CimValidationErrorTextTemplateMessages.Unknown,

            };
        }

        // TODO: Remove when prices are no longer part of a D18 message
        private string GetPosition(ChargeInformationDto chargeInformationDto,  string? triggeredBy)
        {
            var parsed = int.TryParse(triggeredBy, out var position);
            if (!string.IsNullOrWhiteSpace(triggeredBy) && parsed && position > 0)
                return triggeredBy;

            var errorMessage = $"Invalid position ({triggeredBy}) for charge with " +
                               $"id: {chargeInformationDto.ChargeId}," +
                               $"type: {chargeInformationDto.Type}," +
                               $"owner: {chargeInformationDto.ChargeOwner}";
            _logger.LogError("Invalid position: {errorMessage}", errorMessage);

            return CimValidationErrorTextTemplateMessages.Unknown;
        }

        // TODO: Remove when prices are no longer part of a D18 message
        private string GetPriceFromPointByPosition(ChargeInformationDto chargeInformationDto, string? triggeredBy)
        {
            try
            {
                return chargeInformationDto.Points
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

            _logger.LogError("Id for failed information is null");

            return CimValidationErrorTextTemplateMessages.Unknown;
        }
    }
}
