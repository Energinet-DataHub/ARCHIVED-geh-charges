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

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using GreenEnergyHub.Charges.Application.Common.Helpers;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands.Validation.BusinessValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands.Validation.BusinessValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.MessageHub.Models.Shared;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using GreenEnergyHub.TestHelpers;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.MessageHub.Models.Shared
{
    [UnitTest]
    public class ValidationErrorLogMessageBuilderTests
    {
        [Theory]
        [AutoDomainData]
        public void BuildErrorMessage_WhenCalled_ShouldReturnErrorMessage(DocumentDtoBuilder documentDtoBuilder)
        {
            // Arrange
            var documentDto = BuildDocumentDto(documentDtoBuilder);
            var invalidRules = new List<IValidationRuleContainer>()
            {
                new OperationValidationRuleContainer(
                    new ChargeMustExistRule(null), "operationId1"),
                new OperationValidationRuleContainer(
                    new PreviousOperationsMustBeValidRule("operationId1"), "operationId2"),
            };

            var expected = $"document Id {documentDto.Id} with Type {documentDto.Type} from GLN {documentDto.Sender.MarketParticipantId}:\r\n" +
                            $"- ValidationRuleIdentifier: {invalidRules.First().ValidationRule.ValidationRuleIdentifier}\r\n" +
                            $"- ValidationRuleIdentifier: {invalidRules.Last().ValidationRule.ValidationRuleIdentifier}\r\n";

            // Act
            var actual = ValidationErrorLogMessageBuilder.BuildErrorMessage(documentDto, invalidRules);

            // Assert
            actual.Should().Be(expected);
        }

        private static DocumentDto BuildDocumentDto(DocumentDtoBuilder documentDtoBuilder)
        {
            var documentDto = documentDtoBuilder
                .WithDocumentId("00001")
                .WithDocumentType(DocumentType.RequestChangeBillingMasterData)
                .WithSender(new MarketParticipantDto
                {
                    MarketParticipantId = "10000",
                    BusinessProcessRole = MarketParticipantRole.GridAccessProvider,
                })
                .Build();
            return documentDto;
        }
    }
}
