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

using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.BusinessValidation.ValidationRules;
using Xunit;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeCommands.Validation.BusinessValidation.ValidationRules
{
    public class AuthenticatedUserMustMatchMarketParticipantSenderIdRuleTests
    {
        [Theory]
        [InlineAutoMoqData("12345678910", "12345678910", "it is valid as they are the same", true)]
        [InlineAutoMoqData("12345678910", "01987654321", "it is invalid as they are not the same", false)]
        [InlineAutoMoqData(null!, "01987654321", "it is invalid as authentication id is empty", false)]
        public void IsValid_WhenCalled_ShouldReturnTrueIfMarketParticipantIsValid(
            string? authorizedMarketParticipantId,
            string senderMarketParticipantId,
            string because,
            bool expectedIsValid)
        {
            var sut = new AuthenticatedUserMustMatchMarketParticipantSenderIdRule(
                authorizedMarketParticipantId,
                senderMarketParticipantId);

            var isValid = sut.IsValid;

            isValid.Should().Be(expectedIsValid, because);
        }
    }
}
