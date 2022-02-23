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

using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.InputValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Builders;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using GreenEnergyHub.TestHelpers;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeCommands.Validation.InputValidation.ValidationRules
{
    [UnitTest]
    public class DocumentTypeMustBeRequestUpdateChargeInformationRuleTests
    {
        [Theory]
        [InlineAutoMoqData(DocumentType.Unknown, false)]
        [InlineAutoMoqData(DocumentType.RequestUpdateChargeInformation, true)]
        public void DocumentTypeMustBeRequestUpdateChargeInformation_Test(
            DocumentType documentType,
            bool expected,
            ChargeCommandBuilder chargeCommandBuilder)
        {
            var command = CreateCommand(chargeCommandBuilder, documentType);
            var sut = new DocumentTypeMustBeRequestUpdateChargeInformationRule(command);
            sut.IsValid.Should().Be(expected);
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationRuleIdentifier_ShouldBe_EqualTo(ChargeCommandBuilder chargeCommandBuilder)
        {
            var command = CreateCommand(chargeCommandBuilder, DocumentType.Unknown);
            var sut = new DocumentTypeMustBeRequestUpdateChargeInformationRule(command);
            sut.ValidationRuleIdentifier.Should().Be(ValidationRuleIdentifier.DocumentTypeMustBeRequestUpdateChargeInformation);
        }

        private static ChargeCommand CreateCommand(ChargeCommandBuilder builder, DocumentType documentType)
        {
            return builder.WithDocumentType(documentType).Build();
        }
    }
}
