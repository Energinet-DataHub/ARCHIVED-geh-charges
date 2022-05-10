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

namespace GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeLinksReceiptData
{
    public class ChargeLinksCimValidationErrorTextFactory : ICimValidationErrorTextFactory<ChargeLinksCommand, ChargeLinkDto>
    {
        private readonly ICimValidationErrorTextProvider _cimValidationErrorTextProvider;

        public ChargeLinksCimValidationErrorTextFactory(ICimValidationErrorTextProvider cimValidationErrorTextProvider)
        {
            _cimValidationErrorTextProvider = cimValidationErrorTextProvider;
        }

        public string Create(ValidationError validationError, ChargeLinksCommand command, ChargeLinkDto chargeLinkDto)
        {
            return GetMergedErrorMessage(validationError, chargeLinkDto);
        }

        private string GetMergedErrorMessage(
            ValidationError validationError,
            ChargeLinkDto chargeLinkDto)
        {
            var errorTextTemplate = _cimValidationErrorTextProvider
                .GetCimValidationErrorText(validationError.ValidationRuleIdentifier);

            return MergeErrorText(errorTextTemplate, chargeLinkDto);
        }

        private string MergeErrorText(
            string errorTextTemplate,
            ChargeLinkDto chargeLinkDto)
        {
            var tokens = CimValidationErrorTextTokenMatcher.GetTokens(errorTextTemplate);

            var mergedErrorText = errorTextTemplate;

            foreach (var token in tokens)
            {
                var data = GetDataForToken(token, chargeLinkDto);
                mergedErrorText = mergedErrorText.Replace("{{" + token + "}}", data);
            }

            return mergedErrorText;
        }

        private string GetDataForToken(
            CimValidationErrorTextToken token,
            ChargeLinkDto chargeLinkDto)
        {
            // Please keep sorted by CimValidationErrorTextToken
            return token switch
            {
                CimValidationErrorTextToken.ChargeLinkStartDate =>
                    chargeLinkDto.StartDateTime.ToString(),
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
                _ => CimValidationErrorTextTemplateMessages.Unknown,
            };
        }
    }
}
