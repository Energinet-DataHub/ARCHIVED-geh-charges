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
using Energinet.Charges.Contracts.Charge;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using FluentAssertions;
using GreenEnergyHub.Charges.QueryApi.Model;
using GreenEnergyHub.Charges.TestCore.Builders.Query;
using GreenEnergyHub.Charges.WebApi.ModelPredicates;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.WebApi.ModelPredicates
{
    [UnitTest]
    public class ChargeQueryLogicTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void AsChargeV1Dto_SetsAllProperties(Charge charge)
        {
            // Arrange
            charge.OwnerId = charge.Owner.Id;
            charge.Type = 1;
            charge.Resolution = 1;

            charge.ChargePeriods.Clear();
            charge.ChargePeriods.Add(GenerateChargePeriod(charge));

            var charges = new List<Charge> { charge, }.AsQueryable();

            var expected = new ChargeV1Dto(
                (ChargeType)charge.Type,
                (Resolution)charge.Resolution,
                charge.SenderProvidedChargeId,
                charge.ChargePeriods.Single().Name,
                charge.Owner.MarketParticipantId,
                "<Aktørnavn XYZ>",
                charge.TaxIndicator,
                charge.ChargePeriods.Single().TransparentInvoicing,
                charge.ChargePeriods.Single().StartDateTime,
                charge.ChargePeriods.Single().EndDateTime);

            // Act
            var actual = charges.AsChargeV1Dto();

            // Assert
            actual.Single().Should().BeEquivalentTo(expected);
        }

        private static ChargePeriod GenerateChargePeriod(Charge charge)
        {
            var period = new ChargePeriodBuilder().Build(charge);
            return period;
        }
    }
}
