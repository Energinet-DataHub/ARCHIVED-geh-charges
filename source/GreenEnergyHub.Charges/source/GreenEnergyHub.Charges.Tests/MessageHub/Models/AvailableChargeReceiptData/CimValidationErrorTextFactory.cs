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
using FluentAssertions;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeReceiptData;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Xunit;

namespace GreenEnergyHub.Charges.Tests.MessageHub.Models.AvailableChargeReceiptData
{
    public class CimValidationErrorTextFactoryTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void Create_MapsAllReasonCodes(CimValidationErrorTextFactory sut)
        {
            foreach (var reasonCode in Enum.GetValues<ReasonCode>())
            {
                // Assert - ensures that a not-empty reason error text can be obtained for all known reason codes
                sut.Create(reasonCode).Should().NotBeEmpty();
            }
        }
    }
}
