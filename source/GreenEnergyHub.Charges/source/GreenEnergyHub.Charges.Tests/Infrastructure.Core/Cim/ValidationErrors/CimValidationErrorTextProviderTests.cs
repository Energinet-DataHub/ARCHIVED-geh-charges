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

using System;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.ValidationErrors;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Xunit;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Core.Cim.ValidationErrors
{
    public class CimValidationErrorTextProviderTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void GetCimValidationErrorMessage_MapsAllKnownValidationRuleIdentifiers(CimValidationErrorTextProvider sut)
        {
            foreach (var value in Enum.GetValues<ValidationRuleIdentifier>())
            {
                // Assert that create does not throw (ensures that we are mapping all enum values)
                sut.GetCimValidationErrorText(value);
            }
        }
    }
}
