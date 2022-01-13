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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.ValidationErrors;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;

namespace GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeLinksReceiptData
{
    public class ChargeLinksCimValidationErrorTextFactory : ICimValidationErrorTextFactory<ChargeLinksCommand>
    {
        private readonly ICimValidationErrorTextProvider _cimValidationErrorTextProvider;

        public ChargeLinksCimValidationErrorTextFactory(ICimValidationErrorTextProvider cimValidationErrorTextProvider)
        {
            _cimValidationErrorTextProvider = cimValidationErrorTextProvider;
        }

        public string Create(ValidationError validationError, ChargeLinksCommand command)
        {
            return GetMergedErrorMessage(validationError.ValidationRuleIdentifier, command);
        }

        private string GetMergedErrorMessage(ValidationRuleIdentifier validationRuleIdentifier, ChargeLinksCommand chargeLinksCommand)
        {
            var errorTextTemplate = _cimValidationErrorTextProvider
                .GetCimValidationErrorText(validationRuleIdentifier);

            return MergeErrorText(errorTextTemplate, chargeLinksCommand);
        }

        private static string MergeErrorText(string errorTextTemplate, ChargeLinksCommand chargeLinksCommand)
        {
            var tokens = GetTokens(errorTextTemplate);

            var mergedErrorText = errorTextTemplate;

            foreach (var token in tokens)
            {
                var data = GetDataForToken(token, chargeLinksCommand);
                mergedErrorText = mergedErrorText.Replace("{{" + token + "}}", data);
            }

            return mergedErrorText;
        }

        private static string GetDataForToken(CimValidationErrorTextToken token, ChargeLinksCommand chargeLinksCommand)
        {
            // Please keep sorted by CimValidationErrorTextToken
            return token switch
            {
                CimValidationErrorTextToken.MeteringPointEffectiveDate =>
                    "TODO",
                CimValidationErrorTextToken.ChargeStartDateTime =>
                    "TODO",
                CimValidationErrorTextToken.DocumentSenderProvidedChargeId =>
                    "TODO",
                CimValidationErrorTextToken.MeteringPointId =>
                    chargeLinksCommand.MeteringPointId,
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
