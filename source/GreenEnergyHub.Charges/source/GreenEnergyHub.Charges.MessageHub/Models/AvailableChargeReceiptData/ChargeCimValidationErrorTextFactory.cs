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
using System.Text.RegularExpressions;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.ValidationErrors;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;

namespace GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeReceiptData
{
    public class ChargeCimValidationErrorTextFactory : ICimValidationErrorTextFactory<ChargeCommand>
    {
        private readonly ICimValidationErrorTextProvider _cimValidationErrorTextProvider;

        public ChargeCimValidationErrorTextFactory(ICimValidationErrorTextProvider cimValidationErrorTextProvider)
        {
            _cimValidationErrorTextProvider = cimValidationErrorTextProvider;
        }

        public string Create(ValidationError validationError, ChargeCommand chargeCommand)
        {
            return GetMergedErrorMessage(validationError, chargeCommand);
        }

        private string GetMergedErrorMessage(ValidationError validationError, ChargeCommand chargeCommand)
        {
            var errorTextTemplate = _cimValidationErrorTextProvider
                .GetCimValidationErrorText(validationError.ValidationRuleIdentifier);

            return MergeErrorText(errorTextTemplate, chargeCommand, validationError.TriggeredBy);
        }

        private static string MergeErrorText(
            string errorTextTemplate,
            ChargeCommand chargeCommand,
            string? triggeredBy)
        {
            var tokens = GetTokens(errorTextTemplate);

            var mergedErrorText = errorTextTemplate;

            foreach (var token in tokens)
            {
                var data = GetDataForToken(token, chargeCommand, triggeredBy);
                mergedErrorText = mergedErrorText.Replace("{{" + token + "}}", data);
            }

            return mergedErrorText;
        }

        private static string GetDataForToken(
            CimValidationErrorTextToken token,
            ChargeCommand chargeCommand,
            string? triggeredBy)
        {
            // Please keep sorted by CimValidationErrorTextToken
            return token switch
            {
                CimValidationErrorTextToken.ChargeDescription =>
                    chargeCommand.ChargeOperation.ChargeDescription,
                CimValidationErrorTextToken.ChargeName =>
                    chargeCommand.ChargeOperation.ChargeName,
                CimValidationErrorTextToken.ChargeOwner =>
                    chargeCommand.ChargeOperation.ChargeOwner,
                CimValidationErrorTextToken.ChargePointPosition =>
                    triggeredBy ?? string.Empty,
                CimValidationErrorTextToken.ChargePointPrice =>
                    triggeredBy == null ? string.Empty :
                        chargeCommand.ChargeOperation.Points
                        .Single(p => p.Position == int.Parse(triggeredBy)).Price.ToString("N"),
                CimValidationErrorTextToken.ChargePointsCount =>
                    chargeCommand.ChargeOperation.Points.Count.ToString(),
                CimValidationErrorTextToken.ChargeResolution =>
                    chargeCommand.ChargeOperation.Resolution.ToString(),
                CimValidationErrorTextToken.ChargeStartDateTime =>
                    chargeCommand.ChargeOperation.StartDateTime.ToString(),
                CimValidationErrorTextToken.ChargeTaxIndicator =>
                    chargeCommand.ChargeOperation.TaxIndicator.ToString(),
                CimValidationErrorTextToken.ChargeType =>
                    chargeCommand.ChargeOperation.Type.ToString(),
                CimValidationErrorTextToken.ChargeVatClass =>
                    chargeCommand.ChargeOperation.VatClassification.ToString(),
                CimValidationErrorTextToken.DocumentBusinessReasonCode =>
                    chargeCommand.Document.BusinessReasonCode.ToString(),
                CimValidationErrorTextToken.DocumentId =>
                    chargeCommand.Document.Id,
                CimValidationErrorTextToken.DocumentSenderId =>
                    chargeCommand.Document.Sender.Id,
                CimValidationErrorTextToken.DocumentSenderProvidedChargeId =>
                    chargeCommand.ChargeOperation.ChargeId,
                CimValidationErrorTextToken.DocumentType =>
                    chargeCommand.Document.Type.ToString(),
                _ => string.Empty,
            };
        }

        private static IEnumerable<CimValidationErrorTextToken> GetTokens(string errorTextTemplate)
        {
            // regex to match content between {{ and }} inspired by https://stackoverflow.com/a/16538131
            var matchList = Regex.Matches(errorTextTemplate, @"(?<=\{{)[^}]*(?=\}})");
            return matchList.Select(match =>
                (CimValidationErrorTextToken)Enum.Parse(typeof(CimValidationErrorTextToken), match.Value)).ToList();
        }
    }
}
