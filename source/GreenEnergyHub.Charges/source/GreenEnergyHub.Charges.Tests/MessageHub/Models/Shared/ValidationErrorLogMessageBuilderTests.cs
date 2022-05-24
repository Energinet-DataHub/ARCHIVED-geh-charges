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
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.MessageHub.Models.Shared;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeCommands.Validation;
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
            var validationErrors = BuildValidationErrors();

            var expected = $"document Id {documentDto.Id} with Type {documentDto.Type} from GLN {documentDto.Sender.MarketParticipantId}:\r\n" +
                            $"- ValidationRuleIdentifier: {validationErrors.First().ValidationRuleIdentifier}\r\n" +
                            $"- ValidationRuleIdentifier: {validationErrors.Last().ValidationRuleIdentifier}\r\n";

            // Act
            var actual = ValidationErrorLogMessageBuilder.BuildErrorMessage(documentDto, validationErrors);

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

        private static List<ValidationError> BuildValidationErrors()
        {
            var validationErrors = new List<ValidationError>
            {
                new(ValidationRuleIdentifier.ChargeDoesNotExist, null, null),
                new(ValidationRuleIdentifier.SubsequentBundleOperationsFail, null, null),
            };
            return validationErrors;
        }
    }
}
