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
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using Energinet.Charges.Contracts.ChargeLink;
using FluentAssertions;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.QueryApi.Model;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.WebApi.ModelPredicates;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.WebApi
{
    [UnitTest]
    public class ChargeLinkQueryLogicTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void AsChargeLinkDto_WhenChargeLinkContainsEndDefaultEndDate_EndDateReturnsAsNull(ChargeLink chargeLink)
        {
            // Arrange
            chargeLink.Charge.Type = 1;
            chargeLink.EndDateTime = InstantExtensions.GetEndDefault().ToDateTimeUtc();
            var chargeLinks = new List<ChargeLink>
            {
                chargeLink,
            }.AsQueryable();

            // Act
            var actual = chargeLinks.AsChargeLinkDto();

            actual.Should().NotContain(c => c.EndDate == InstantExtensions.GetEndDefault().ToDateTimeUtc());
        }
    }
}
