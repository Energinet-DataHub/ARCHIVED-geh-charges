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
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands;
using GreenEnergyHub.Charges.Domain.Dtos.Messages.Command;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.ValidationErrors;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;
using GreenEnergyHub.Charges.MessageHub.Models.Shared;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.MessageHub.Models.AvailableOperationReceiptData
{
    public class ChargePriceCimValidationErrorTextFactory : ICimValidationErrorTextFactory<ChargePriceOperationDto>
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

        public string Create(
            ValidationError validationError,
            DocumentDto document,
            ChargePriceOperationDto operationDtoDto)
        {
            return GetMergedErrorMessage(validationError, document, operationDtoDto);
        }

        private string GetMergedErrorMessage(
            ValidationError validationError,
            DocumentDto document,
            ChargePriceOperationDto operationDtoDto)
        {
            var errorTextTemplate = _cimValidationErrorTextProvider
                .GetCimValidationErrorText(validationError.ValidationRuleIdentifier);

            return MergeErrorText(
                errorTextTemplate, document, operationDtoDto, validationError.TriggeredBy);
        }

        private string MergeErrorText(
            string errorTextTemplate,
            DocumentDto document,
            ChargePriceOperationDto operationDtoDto,
            string? triggeredBy)
        {
            var tokens = CimValidationErrorTextTokenMatcher.GetTokens(errorTextTemplate);

            var mergedErrorText = errorTextTemplate;

            foreach (var token in tokens)
            {
                var data = GetDataForToken(token, document, operationDtoDto, triggeredBy);
                mergedErrorText = mergedErrorText.Replace("{{" + token + "}}", data);
            }

            return mergedErrorText;
        }

        private string GetDataForToken(
            CimValidationErrorTextToken token,
            DocumentDto document,
            ChargePriceOperationDto operationDtoDto,
            string? triggeredBy)
        {
            // Please keep sorted by CimValidationErrorTextToken
            return token switch
            {
                CimValidationErrorTextToken.ChargeOwner =>
                    operationDtoDto.ChargeOwner,
                CimValidationErrorTextToken.ChargePointPosition =>
                    GetPosition(operationDtoDto, triggeredBy),
                CimValidationErrorTextToken.ChargePointPrice =>
                    GetPriceFromPointByPosition(operationDtoDto, triggeredBy),
                CimValidationErrorTextToken.ChargePointsCount =>
                    operationDtoDto.Points.Count.ToString(),
                CimValidationErrorTextToken.ChargeResolution =>
                    operationDtoDto.Resolution.ToString(),
                CimValidationErrorTextToken.ChargeType =>
                    operationDtoDto.ChargeType.ToString(),
                CimValidationErrorTextToken.DocumentBusinessReasonCode =>
                    BusinessReasonCode.UpdateChargePrices.ToString(),
                CimValidationErrorTextToken.DocumentId =>
                    document.Id,
                CimValidationErrorTextToken.DocumentSenderId =>
                    document.Sender.MarketParticipantId,
                CimValidationErrorTextToken.DocumentSenderProvidedChargeId =>
                    operationDtoDto.SenderProvidedChargeId,
                CimValidationErrorTextToken.DocumentType =>
                    document.Type.ToString(),
                CimValidationErrorTextToken.TriggeredByOperationId =>
                    GetOperationIdFromTriggeredBy(triggeredBy),
                CimValidationErrorTextToken.ChargeOperationId =>
                    operationDtoDto.OperationId,
                CimValidationErrorTextToken.DocumentRecipientBusinessProcessRole =>
                    document.Recipient.BusinessProcessRole.ToString(),
                _ => CimValidationErrorTextTemplateMessages.Unknown,

            };
        }

        private string GetPosition(ChargeOperationDto operationDtoDto,  string? triggeredBy)
        {
            var parsed = int.TryParse(triggeredBy, out var position);
            if (!string.IsNullOrWhiteSpace(triggeredBy) && parsed && position > 0)
                return triggeredBy;

            var errorMessage = $"Invalid position ({triggeredBy}) for charge with " +
                               $"id: {operationDtoDto.SenderProvidedChargeId}," +
                               $"type: {operationDtoDto.ChargeType}," +
                               $"owner: {operationDtoDto.ChargeOwner}";
            _logger.LogError("Invalid position: {ErrorMessage}", errorMessage);

            return CimValidationErrorTextTemplateMessages.Unknown;
        }

        private string GetPriceFromPointByPosition(ChargePriceOperationDto operationDtoDto, string? triggeredBy)
        {
            try
            {
                return operationDtoDto.Points
                        .Single(p => operationDtoDto.Points.GetPositionOfPoint(p) == int.Parse(triggeredBy!))
                        .Price.ToString("N");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Price not found {ErrorMessage}", $"by position: {triggeredBy}");

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
