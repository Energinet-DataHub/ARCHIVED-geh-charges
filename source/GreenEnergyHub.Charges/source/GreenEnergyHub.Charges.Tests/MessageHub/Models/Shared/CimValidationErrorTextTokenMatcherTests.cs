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
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.ValidationErrors;
using GreenEnergyHub.Charges.MessageHub.Models.Shared;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Xunit;

namespace GreenEnergyHub.Charges.Tests.MessageHub.Models.Shared
{
    public class CimValidationErrorTextTokenMatcherTests
    {
        [Theory]
        [InlineAutoMoqData(
            CimValidationErrorTextTemplateMessages.VatClassificationValidationErrorText,
            CimValidationErrorTextToken.DocumentSenderProvidedChargeId,
            CimValidationErrorTextToken.ChargeOwner,
            CimValidationErrorTextToken.ChargeType)]
        public void GetTokens_Returns_ExpectedTokens(
            string errorText,
            CimValidationErrorTextToken token1,
            CimValidationErrorTextToken token2,
            CimValidationErrorTextToken token3)
        {
            var actual = CimValidationErrorTextTokenMatcher.GetTokens(errorText);

            actual.Should().Contain(token1).And.Contain(token2).And.Contain(token3);
        }
    }
}
