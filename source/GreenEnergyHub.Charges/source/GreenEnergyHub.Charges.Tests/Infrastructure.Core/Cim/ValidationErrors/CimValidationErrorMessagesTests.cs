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
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Kernel;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.BusinessValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.InputValidation.ValidationRules;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.ValidationErrors;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Core.Cim.ValidationErrors
{
    [UnitTest]
    public class CimValidationErrorMessagesTests
    {
        [Theory]
        [InlineAutoMoqData(CimValidationErrorTemplateMessages.UnknownError, null!)]
        [InlineAutoMoqData(CimValidationErrorTemplateMessages.StartDateValidationErrorMessage, typeof(StartDateValidationRule))]
        [InlineAutoMoqData(CimValidationErrorTemplateMessages.ChangingTariffVatValueNotAllowedErrorMessage, typeof(ChangingTariffVatValueNotAllowedRule))]
        [InlineAutoMoqData(CimValidationErrorTemplateMessages.ChangingTariffTaxValueNotAllowedErrorMessage, typeof(ChangingTariffTaxValueNotAllowedRule))]
        [InlineAutoMoqData(CimValidationErrorTemplateMessages.ProcessTypeIsKnownValidationErrorMessage, typeof(ProcessTypeIsKnownValidationRule))]
        [InlineAutoMoqData(CimValidationErrorTemplateMessages.SenderIsMandatoryTypeValidationErrorMessage, typeof(SenderIsMandatoryTypeValidationRule))]
        [InlineAutoMoqData(CimValidationErrorTemplateMessages.RecipientIsMandatoryTypeValidationErrorMessage, typeof(RecipientIsMandatoryTypeValidationRule))]
        [InlineAutoMoqData(CimValidationErrorTemplateMessages.ChargeOperationIdRequiredErrorMessage, typeof(ChargeOperationIdRequiredRule))]
        // TODO BJARKE: There is no corresponding validation rule for OperationTypeValidationErrorMessage
        //[InlineAutoMoqData(CimValidationErrorTemplateMessages.OperationTypeValidationErrorMessage, typeof(xValidationRule))]
        [InlineAutoMoqData(CimValidationErrorTemplateMessages.ChargeIdLengthValidationErrorMessage, typeof(ChargeIdLengthValidationRule))]
        [InlineAutoMoqData(CimValidationErrorTemplateMessages.ChargeIdRequiredValidationErrorMessage, typeof(ChargeIdRequiredValidationRule))]
        [InlineAutoMoqData(CimValidationErrorTemplateMessages.DocumentTypeMustBeRequestUpdateChargeInformationErrorMessage, typeof(DocumentTypeMustBeRequestUpdateChargeInformationRule))]
        [InlineAutoMoqData(CimValidationErrorTemplateMessages.BusinessReasonCodeMustBeUpdateChargeInformationErrorMessage, typeof(BusinessReasonCodeMustBeUpdateChargeInformationRule))]
        [InlineAutoMoqData(CimValidationErrorTemplateMessages.ChargeTypeIsKnownValidationErrorMessage, typeof(ChargeTypeIsKnownValidationRule))]
        [InlineAutoMoqData(CimValidationErrorTemplateMessages.VatClassificationValidationErrorMessage, typeof(VatClassificationValidationRule))]
        [InlineAutoMoqData(CimValidationErrorTemplateMessages.ResolutionTariffValidationErrorMessage, typeof(ResolutionTariffValidationRule))]
        [InlineAutoMoqData(CimValidationErrorTemplateMessages.ResolutionFeeValidationErrorMessage, typeof(ResolutionFeeValidationRule))]
        [InlineAutoMoqData(CimValidationErrorTemplateMessages.ResolutionSubscriptionValidationErrorMessage, typeof(ResolutionSubscriptionValidationRule))]
        [InlineAutoMoqData(CimValidationErrorTemplateMessages.StartDateTimeRequiredValidationErrorMessage, typeof(StartDateTimeRequiredValidationRule))]
        [InlineAutoMoqData(CimValidationErrorTemplateMessages.ChargeOwnerIsRequiredValidationErrorMessage, typeof(ChargeOwnerIsRequiredValidationRule))]
        [InlineAutoMoqData(CimValidationErrorTemplateMessages.ChargeNameHasMaximumLengthErrorMessage, typeof(ChargeNameHasMaximumLengthRule))]
        [InlineAutoMoqData(CimValidationErrorTemplateMessages.ChargeDescriptionHasMaximumLengthErrorMessage, typeof(ChargeDescriptionHasMaximumLengthRule))]
        [InlineAutoMoqData(CimValidationErrorTemplateMessages.ChargeTypeTariffPriceCountErrorMessage, typeof(ChargeTypeTariffPriceCountRule))]
        [InlineAutoMoqData(CimValidationErrorTemplateMessages.MaximumPriceErrorMessage, typeof(MaximumPriceRule))]
        [InlineAutoMoqData(CimValidationErrorTemplateMessages.ChargePriceMaximumDigitsAndDecimalsErrorMessage, typeof(ChargePriceMaximumDigitsAndDecimalsRule))]
        [InlineAutoMoqData(CimValidationErrorTemplateMessages.FeeMustHaveSinglePriceErrorMessage, typeof(FeeMustHaveSinglePriceRule))]
        [InlineAutoMoqData(CimValidationErrorTemplateMessages.SubscriptionMustHaveSinglePriceErrorMessage, typeof(SubscriptionMustHaveSinglePriceRule))]
        [InlineAutoMoqData(CimValidationErrorTemplateMessages.CommandSenderMustBeAnExistingMarketParticipantErrorMessage, typeof(CommandSenderMustBeAnExistingMarketParticipantRule))]
        [InlineAutoMoqData(CimValidationErrorTemplateMessages.UpdateNotYetSupportedErrorMessage, typeof(ChargeUpdateNotYetSupportedRule))]
        public void CimValidationErrorTemplateMessage_HasPlaceHoldersMatchingTheValidationRuleParameters(string actual, Type? ruleType)
        {
            var rule = CreateValidationRule(ruleType);
            AssertPlaceHolders(actual, rule);
        }

        private static IValidationRule? CreateValidationRule(Type? ruleType)
        {
            if (ruleType == null) return null;

            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            return (IValidationRule)fixture.Create(ruleType, new SpecimenContext(fixture));
        }

        private static void AssertPlaceHolders(string actual, IValidationRule? rule)
        {
            var actualPlaceHolderCount = GetPlaceHolderCount(actual);
            if (rule == null)
            {
                actualPlaceHolderCount.Should().Be(0);
                return;
            }

            var expectedPlaceHolderCount = rule.ValidationError!.ValidationErrorMessageParameters.Count;
            actualPlaceHolderCount.Should().Be(expectedPlaceHolderCount);

            foreach (var x in rule.ValidationError.ValidationErrorMessageParameters)
            {
                var expectedPlaceHolder = "{{" + x.ParameterType + "}}";
                actual.Should().Contain(expectedPlaceHolder);
            }
        }

        private static int GetPlaceHolderCount(string actual)
        {
            // This in an heuristic
            return actual.Split("{{").Length - 1;
        }
    }
}
