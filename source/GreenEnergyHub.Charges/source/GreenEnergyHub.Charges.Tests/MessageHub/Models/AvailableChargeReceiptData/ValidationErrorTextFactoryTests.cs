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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeReceiptData;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.MessageHub.Models.AvailableChargeReceiptData
{
    [UnitTest]
    public class ValidationErrorTextFactoryTests
    {
        // TODO BJARKE
        // [Theory]
        // [InlineData(ValidationRuleIdentifier.MaximumPrice, "Price {{$mergeField1}} not allowed: The specified charge price for position {{$mergeField2}} is not plausible (too large)")]
        // public void Create_ReturnsExpectedText(ValidationRuleIdentifier ruleIdentifier, params ValidationErrorMessageParameter[] messageParameters)
        // {
        //     var error = new ValidationError(ruleIdentifier, messageParameters);
        //     var sut = new ValidationErrorTextFactory();
        //     var actual = sut.Create(error);
        //     actual.Should().Be("xxx");
        // }
        // [Theory]
        // [InlineAutoMoqData]
        // public void Create_WhenTwoMergeFields_ReturnsExpectedText(ValidationErrorTextFactory sut)
        // {
        //     var ruleIdentifierWithTwoMergeFields = ValidationRuleIdentifier.MaximumPrice;
        //
        //     var error = new ValidationError(ruleIdentifierWithTwoMergeFields, );
        //     var actual = sut.Create(error);
        //     actual.Should().Be("xxx");
        // }
    }
}
