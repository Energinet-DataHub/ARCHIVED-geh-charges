﻿// Copyright 2020 Energinet DataHub A/S
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

using System.Diagnostics.CodeAnalysis;
using GreenEnergyHub.Charges.Application.Validation.InputValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using Xunit;

namespace GreenEnergyHub.Charges.Tests.Application.Validation.InputValidation.ValidationRules
{
    public class ChargeIdRequiredValidationRuleTest
    {
        [Theory]
        [InlineAutoMoqData("ChargeId", true)]
        [InlineAutoMoqData("", false)]
        [InlineAutoMoqData(" ", false)]
        [InlineAutoMoqData(null!, false)]
        public void Test(
            string chargeId,
            bool expected,
            [NotNull] ChargeCommand command)
        {
            command.ChargeOperation.ChargeId = chargeId;
            var sut = new ChargeIdRequiredValidationRule(command);
            Assert.Equal(expected, sut.IsValid);
        }
    }
}
