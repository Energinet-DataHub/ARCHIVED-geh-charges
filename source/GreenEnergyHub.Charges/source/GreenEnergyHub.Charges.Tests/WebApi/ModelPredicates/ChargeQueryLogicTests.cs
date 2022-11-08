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
using Energinet.DataHub.Charges.Contracts.Charge;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using FluentAssertions;
using GreenEnergyHub.Charges.QueryApi.Model;
using GreenEnergyHub.Charges.QueryApi.ModelPredicates;
using GreenEnergyHub.Charges.TestCore.Builders.Query;
using GreenEnergyHub.Charges.TestCore.TestHelpers;
using NodaTime;
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
            charge.ChargePeriods.Add(GenerateChargePeriod(charge, InstantHelper.GetTodayAtMidnightUtc(), InstantHelper.GetEndDefault()));

            var charges = new List<Charge> { charge, }.AsQueryable();

            var expected = new ChargeV1Dto(
                charge.Id,
                (ChargeType)charge.Type,
                (Resolution)charge.Resolution,
                charge.SenderProvidedChargeId,
                charge.ChargePeriods.Single().Name,
                charge.ChargePeriods.Single().Description,
                charge.Owner.MarketParticipantId,
                charge.Owner.Name,
                (VatClassification)charge.ChargePeriods.Single().VatClassification,
                charge.TaxIndicator,
                charge.ChargePeriods.Single().TransparentInvoicing,
                charge.ChargePoints.Any(),
                charge.ChargePeriods.Single().StartDateTime,
                null);

            // Act
            var actual = charges.AsChargeV1Dto();

            // Assert
            actual.Single().Should().BeEquivalentTo(expected);
        }

        [Theory]
        [InlineAutoMoqData]
        public void SelectManyAsChargeV1Dto_SetsAllProperties(Charge charge)
        {
            // Arrange
            charge.OwnerId = charge.Owner.Id;
            charge.Type = 1;
            charge.Resolution = 1;

            charge.ChargePeriods.Clear();
            AddTwoChargePeriods(charge);

            var charges = new List<Charge> { charge }.AsQueryable();

            var expectedFirst = new ChargeV1Dto(
                charge.Id,
                (ChargeType)charge.Type,
                (Resolution)charge.Resolution,
                charge.SenderProvidedChargeId,
                charge.ChargePeriods.First().Name,
                charge.ChargePeriods.First().Description,
                charge.Owner.MarketParticipantId,
                charge.Owner.Name,
                (VatClassification)charge.ChargePeriods.First().VatClassification,
                charge.TaxIndicator,
                charge.ChargePeriods.First().TransparentInvoicing,
                charge.ChargePoints.Any(),
                charge.ChargePeriods.First().StartDateTime,
                charge.ChargePeriods.First().EndDateTime);

            var expectedLast = new ChargeV1Dto(
                charge.Id,
                (ChargeType)charge.Type,
                (Resolution)charge.Resolution,
                charge.SenderProvidedChargeId,
                charge.ChargePeriods.Last().Name,
                charge.ChargePeriods.Last().Description,
                charge.Owner.MarketParticipantId,
                charge.Owner.Name,
                (VatClassification)charge.ChargePeriods.Last().VatClassification,
                charge.TaxIndicator,
                charge.ChargePeriods.Last().TransparentInvoicing,
                charge.ChargePoints.Any(),
                charge.ChargePeriods.Last().StartDateTime,
                null);

            // Act
            var actual = charges.SelectManyAsChargeV1Dto();

            // Assert
            actual.Should().HaveCount(2);
            actual.First().Should().BeEquivalentTo(expectedFirst);
            actual.Last().Should().BeEquivalentTo(expectedLast);
        }

        private static void AddTwoChargePeriods(Charge charge)
        {
            var firstPeriod = GenerateChargePeriod(charge, InstantHelper.GetTodayAtMidnightUtc(), InstantHelper.GetTomorrowAtMidnightUtc());
            var secondPeriod = GenerateChargePeriod(charge, InstantHelper.GetTomorrowAtMidnightUtc(), InstantHelper.GetEndDefault());

            charge.ChargePeriods.Add(firstPeriod);
            charge.ChargePeriods.Add(secondPeriod);
        }

        private static ChargePeriod GenerateChargePeriod(Charge charge, Instant validFrom, Instant validTo)
        {
            var period = new ChargePeriodBuilder()
                .WithStartDateTime(validFrom.ToDateTimeUtc())
                .WithEndDateTime(validTo.ToDateTimeUtc())
                .WithVatClassification(GreenEnergyHub.Charges.Domain.Charges.VatClassification.Vat25)
                .Build(charge);
            return period;
        }
    }
}
