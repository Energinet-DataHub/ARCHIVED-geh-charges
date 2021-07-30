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
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Acknowledgements;
using GreenEnergyHub.Charges.Infrastructure.Integration.ChargeConfirmation;
using GreenEnergyHub.Charges.Infrastructure.Integration.Mappers;
using GreenEnergyHub.Charges.TestCore;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Integration.Mappers
{
    [UnitTest]
    public class ChargeConfirmationOutboundMapperTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void Convert_WhenCalled_MapsToCorrectValues(
            [NotNull] ChargeConfirmation chargeConfirmation,
            [NotNull] ChargeConfirmationOutboundMapper sut)
        {
            // Act
            var result = (ChargeConfirmationContract)sut.Convert(chargeConfirmation);
            AssertExtensions.ContractIsEquivalent(result, chargeConfirmation);
        }

        [Fact]
        public void Convert_WhenCalledWithNull_ShouldThrow()
        {
            var mapper = new ChargeConfirmationOutboundMapper();
            ChargeConfirmation? chargeConfirmation = null;
            Assert.Throws<InvalidOperationException>(() => mapper.Convert(chargeConfirmation!));
        }
    }
}
