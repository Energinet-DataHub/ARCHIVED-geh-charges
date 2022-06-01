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
    // TODO: Add ChargePriceCimValidationErrorTextFactoryTests
    public class ChargePriceCimValidationErrorTextFactory : ICimValidationErrorTextFactory<ChargeCommand, ChargePriceDto>
    {
        private readonly ICimValidationErrorTextProvider _cimValidationErrorTextProvider;
        private readonly ILogger _logger;

        public ChargePriceCimValidationErrorTextFactory(
            ICimValidationErrorTextProvider cimValidationErrorTextProvider,
            ILoggerFactory loggerFactory)
        {
            _cimValidationErrorTextProvider = cimValidationErrorTextProvider;
            _logger = loggerFactory.CreateLogger(nameof(ChargePriceCimValidationErrorTextFactory));
        }

        public string Create(ValidationError validationError, ChargeCommand command, ChargePriceDto chargePriceDto)
        {
            return GetMergedErrorMessage(validationError, command, chargePriceDto);
        }

        private string GetMergedErrorMessage(
            ValidationError validationError,
            ChargeCommand chargeCommand,
            ChargePriceDto chargePriceDto)
        {
            var errorTextTemplate = _cimValidationErrorTextProvider
                .GetCimValidationErrorText(validationError.ValidationRuleIdentifier);

            return MergeErrorText(errorTextTemplate, chargeCommand, chargePriceDto, validationError.TriggeredBy);
        }

        private string MergeErrorText(
            string errorTextTemplate,
            ChargeCommand chargeCommand,
            ChargePriceDto chargePriceDto,
            string? triggeredBy)
        {
            var tokens = CimValidationErrorTextTokenMatcher.GetTokens(errorTextTemplate);

            var mergedErrorText = errorTextTemplate;

            foreach (var token in tokens)
            {
                var data = GetDataForToken(token, chargeCommand, chargePriceDto, triggeredBy);
                mergedErrorText = mergedErrorText.Replace("{{" + token + "}}", data);
            }

            return mergedErrorText;
        }

        private string GetDataForToken(
            CimValidationErrorTextToken token,
            ChargeCommand chargeCommand,
            ChargePriceDto chargePriceDto,
            string? triggeredBy)
        {
            // Please keep sorted by CimValidationErrorTextToken
            return token switch
            {
                CimValidationErrorTextToken.ChargeOwner =>
                    chargePriceDto.ChargeOwner,
                CimValidationErrorTextToken.ChargePointPosition =>
                    GetPosition(chargePriceDto, triggeredBy),
                CimValidationErrorTextToken.ChargePointPrice =>
                    GetPriceFromPointByPosition(chargePriceDto, triggeredBy),
                CimValidationErrorTextToken.ChargePointsCount =>
                    chargePriceDto.Points.Count.ToString(),
                CimValidationErrorTextToken.ChargeStartDateTime =>
                    chargePriceDto.StartDateTime.ToString(),
                CimValidationErrorTextToken.ChargeType =>
                    chargePriceDto.Type.ToString(),
                CimValidationErrorTextToken.DocumentBusinessReasonCode =>
                    chargeCommand.Document.BusinessReasonCode.ToString(),
                CimValidationErrorTextToken.DocumentId =>
                    chargeCommand.Document.Id,
                CimValidationErrorTextToken.DocumentSenderId =>
                    chargeCommand.Document.Sender.Id,
                CimValidationErrorTextToken.DocumentSenderProvidedChargeId =>
                    chargePriceDto.ChargeId,
                CimValidationErrorTextToken.DocumentType =>
                    chargeCommand.Document.Type.ToString(),
                CimValidationErrorTextToken.TriggeredByOperationId =>
                    GetOperationIdFromTriggeredBy(triggeredBy),
                CimValidationErrorTextToken.ChargeOperationId =>
                    chargePriceDto.Id,
                CimValidationErrorTextToken.DocumentRecipientBusinessProcessRole =>
                    chargeCommand.Document.Recipient.BusinessProcessRole.ToString(),
                _ => CimValidationErrorTextTemplateMessages.Unknown,

            };
        }

        private string GetPosition(ChargePriceDto chargePriceDto,  string? triggeredBy)
        {
            var parsed = int.TryParse(triggeredBy, out var position);
            if (!string.IsNullOrWhiteSpace(triggeredBy) && parsed && position > 0)
                return triggeredBy;

            var errorMessage = $"Invalid position ({triggeredBy}) for charge with " +
                               $"id: {chargePriceDto.ChargeId}," +
                               $"type: {chargePriceDto.Type}," +
                               $"owner: {chargePriceDto.ChargeOwner}";
            _logger.LogError("Invalid position: {errorMessage}", errorMessage);

            return CimValidationErrorTextTemplateMessages.Unknown;
        }

        private string GetPriceFromPointByPosition(ChargePriceDto chargePriceDto, string? triggeredBy)
        {
            try
            {
                return chargePriceDto.Points
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
