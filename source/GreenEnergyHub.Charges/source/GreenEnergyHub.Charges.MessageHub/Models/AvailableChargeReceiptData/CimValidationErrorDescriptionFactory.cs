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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.ValidationErrors;

namespace GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeReceiptData
{
    public class CimValidationErrorDescriptionFactory : ICimValidationErrorDescriptionFactory
    {
        private readonly ICimValidationErrorDescriptionProvider _cimValidationErrorDescriptionProvider;

        public CimValidationErrorDescriptionFactory(ICimValidationErrorDescriptionProvider cimValidationErrorDescriptionProvider)
        {
            _cimValidationErrorDescriptionProvider = cimValidationErrorDescriptionProvider;
        }

        public string Create(ValidationRuleIdentifier validationRuleIdentifier, ChargeCommand chargeCommand)
        {
            return GetMergedErrorMessage(validationRuleIdentifier, chargeCommand);
        }

        private string GetMergedErrorMessage(ValidationRuleIdentifier validationRuleIdentifier, ChargeCommand chargeCommand)
        {
            var errorDescriptionTemplate = _cimValidationErrorDescriptionProvider
                .GetCimValidationErrorDescription(validationRuleIdentifier);

            return MergeErrorDescription(errorDescriptionTemplate, chargeCommand);
        }

        private static string MergeErrorDescription(string errorDescriptionTemplate, ChargeCommand chargeCommand)
        {
            var tokens = GetTokens(errorDescriptionTemplate);

            var mergedErrorDescription = errorDescriptionTemplate;

            foreach (var token in tokens)
            {
                var data = GetDataForToken(token, chargeCommand);
                mergedErrorDescription = mergedErrorDescription.Replace("{{" + token + "}}", data);
            }

            return mergedErrorDescription;
        }

        private static string GetDataForToken(CimValidationErrorDescriptionToken token, ChargeCommand chargeCommand)
        {
            // Please keep switch sorted alphabetically by CimValidationErrorDescriptionToken
            return token switch
            {
                CimValidationErrorDescriptionToken.ChargeDescription =>
                    chargeCommand.ChargeOperation.ChargeDescription,
                CimValidationErrorDescriptionToken.ChargeName =>
                    chargeCommand.ChargeOperation.ChargeName,
                CimValidationErrorDescriptionToken.ChargeOwner =>
                    chargeCommand.ChargeOperation.ChargeOwner,
                CimValidationErrorDescriptionToken.ChargePointPosition =>
                    "To be implemented in upcoming pull request", // TODO: Henrik
                CimValidationErrorDescriptionToken.ChargePointPrice =>
                    "To be implemented in upcoming pull request", // TODO: Henrik
                CimValidationErrorDescriptionToken.ChargePointsCount =>
                    chargeCommand.ChargeOperation.Points.Count.ToString(),
                CimValidationErrorDescriptionToken.ChargeResolution =>
                    chargeCommand.ChargeOperation.Resolution.ToString(),
                CimValidationErrorDescriptionToken.ChargeStartDateTime =>
                    chargeCommand.ChargeOperation.StartDateTime.ToString(),
                CimValidationErrorDescriptionToken.ChargeTaxIndicator =>
                    chargeCommand.ChargeOperation.TaxIndicator.ToString(),
                CimValidationErrorDescriptionToken.ChargeType =>
                    chargeCommand.ChargeOperation.Type.ToString(),
                CimValidationErrorDescriptionToken.ChargeVatClass =>
                    chargeCommand.ChargeOperation.VatClassification.ToString(),
                CimValidationErrorDescriptionToken.DocumentBusinessReasonCode =>
                    chargeCommand.Document.BusinessReasonCode.ToString(),
                CimValidationErrorDescriptionToken.DocumentId =>
                    chargeCommand.Document.Id,
                CimValidationErrorDescriptionToken.DocumentSenderId =>
                    chargeCommand.Document.Sender.Id,
                CimValidationErrorDescriptionToken.DocumentSenderProvidedChargeId =>
                    chargeCommand.ChargeOperation.ChargeId,
                CimValidationErrorDescriptionToken.DocumentType =>
                    chargeCommand.Document.Type.ToString(),
                _ => string.Empty
            };
        }

        private static IEnumerable<CimValidationErrorDescriptionToken> GetTokens(string errorDescriptionTemplate)
        {
            // regex to match content between {{ and }} inspired by https://stackoverflow.com/a/16538131
            var matchList = Regex.Matches(errorDescriptionTemplate, @"(?<=\{{)[^}]*(?=\}})");
            return matchList.Select(match =>
                (CimValidationErrorDescriptionToken)Enum.Parse(typeof(CimValidationErrorDescriptionToken), match.Value))
                .ToList();
        }
    }
}
