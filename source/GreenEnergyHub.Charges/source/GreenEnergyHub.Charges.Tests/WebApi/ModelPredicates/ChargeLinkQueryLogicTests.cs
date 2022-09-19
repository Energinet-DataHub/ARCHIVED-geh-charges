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
using Energinet.Charges.Contracts.Charge;
using Energinet.Charges.Contracts.ChargeLink;
using FluentAssertions;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.QueryApi.Model;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.TestCore.Builders.Query;
using GreenEnergyHub.Charges.WebApi.ModelPredicates;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.WebApi.ModelPredicates
{
    [UnitTest]
    public class ChargeLinkQueryLogicTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void AsChargeLinkV1Dto_SetsAllProperties(ChargeLink chargeLink)
        {
            // Arrange
            chargeLink.Charge.Type = 1;
            chargeLink.Charge.OwnerId = chargeLink.Charge.Owner.Id;

            chargeLink.Charge.ChargePeriods.Clear();
            chargeLink.Charge.ChargePeriods.Add(GenerateChargePeriod(chargeLink.Charge));

            var chargeLinks = new List<ChargeLink> { chargeLink, }.AsQueryable();

            var expected = new ChargeLinkV1Dto(
                (ChargeType)chargeLink.Charge.Type,
                chargeLink.Charge.SenderProvidedChargeId,
                chargeLink.Charge.ChargePeriods.Single().Name,
                chargeLink.Charge.Owner.MarketParticipantId,
                "<Aktørnavn XYZ>",
                chargeLink.Charge.TaxIndicator,
                chargeLink.Charge.ChargePeriods.Single().TransparentInvoicing,
                chargeLink.Factor,
                chargeLink.StartDateTime,
                chargeLink.EndDateTime);

            // Act
            var actual = chargeLinks.AsChargeLinkV1Dto();

            // Assert
            actual.Single().Should().BeEquivalentTo(expected);
        }

        [Theory]
        [InlineAutoMoqData]
        public void AsChargeLinkV1Dto_WhenChargeLinkContainsEndDefaultEndDate_EndDateReturnsAsNull(ChargeLink chargeLink)
        {
            // Arrange
            chargeLink.Charge.Type = 1;
            chargeLink.EndDateTime = InstantExtensions.GetEndDefault().ToDateTimeUtc();
            chargeLink.Charge.ChargePeriods.Clear();
            chargeLink.Charge.ChargePeriods.Add(GenerateChargePeriod(chargeLink.Charge));

            var chargeLinks = new List<ChargeLink> { chargeLink, }.AsQueryable();

            // Act
            var actual = chargeLinks.AsChargeLinkV1Dto();

            // Assert
            actual.Should().HaveCount(1);
            actual.Should().Contain(c => c.EndDate == null);
        }

        [Theory]
        [InlineAutoMoqData(false, "SecondPeriodName", false)]
        [InlineAutoMoqData(true, "ThirdPeriodName", true)]
        public void AsChargeLinkV1Dto_WhenChargeLinkHasChargeWithManyChargePeriods_UsesCurrentPeriodValues(
            bool includePeriodStartingToday, string expectedName, bool expectedTransparentInvoicing, ChargeLink chargeLink)
        {
            // Arrange
            chargeLink.Charge.Type = 1;
            chargeLink.Charge.ChargePeriods.Clear();

            var chargePeriods = GenerateChargePeriods(chargeLink.Charge, includePeriodStartingToday);
            var chargeLinks = BuildChargeLinksWithChargesPeriods(chargeLink, chargePeriods);

            // Act
            var actual = chargeLinks.AsChargeLinkV1Dto();

            // Assert
            actual.Single().ChargeName.Should().Be(expectedName);
            actual.Single().TransparentInvoicing.Should().Be(expectedTransparentInvoicing);
        }

        [Theory]
        [InlineAutoMoqData("FirstPeriodName", true)]
        public void AsChargeLinkV1Dto_WhenChargeLinkHasChargeWithOnlyFutureChargePeriods_UsesFirstPeriodValues(
            string expectedName, bool expectedTransparentInvoicing, ChargeLink chargeLink)
        {
            // Arrange
            chargeLink.Charge.Type = 1;
            chargeLink.Charge.ChargePeriods.Clear();

            var chargePeriods = GenerateFutureChargePeriods(chargeLink.Charge);
            var chargeLinks = BuildChargeLinksWithChargesPeriods(chargeLink, chargePeriods);

            // Act
            var actual = chargeLinks.AsChargeLinkV1Dto();

            // Assert
            actual.Single().ChargeName.Should().Be(expectedName);
            actual.Single().TransparentInvoicing.Should().Be(expectedTransparentInvoicing);
        }

        [Theory]
        [InlineAutoMoqData]
        public void AsChargeLinkV2Dto_SetsAllProperties(ChargeLink chargeLink)
        {
            // Arrange
            chargeLink.Charge.Type = 1;
            chargeLink.Charge.OwnerId = chargeLink.Charge.Owner.Id;

            chargeLink.Charge.ChargePeriods.Clear();
            chargeLink.Charge.ChargePeriods.Add(GenerateChargePeriod(chargeLink.Charge));

            var chargeLinks = new List<ChargeLink> { chargeLink, }.AsQueryable();

            var expected = new ChargeLinkV2Dto(
                (ChargeType)chargeLink.Charge.Type,
                chargeLink.Charge.SenderProvidedChargeId,
                chargeLink.Charge.ChargePeriods.Single().Name,
                chargeLink.Charge.OwnerId,
                chargeLink.Charge.TaxIndicator,
                chargeLink.Charge.ChargePeriods.Single().TransparentInvoicing,
                chargeLink.Factor,
                chargeLink.StartDateTime,
                chargeLink.EndDateTime);

            // Act
            var actual = chargeLinks.AsChargeLinkV2Dto();

            // Assert
            actual.Single().Should().BeEquivalentTo(expected);
        }

        [Theory]
        [InlineAutoMoqData]
        public void AsChargeLinkV2Dto_WhenChargeLinkContainsEndDefaultEndDate_EndDateReturnsAsNull(ChargeLink chargeLink)
        {
            // Arrange
            chargeLink.Charge.Type = 1;
            chargeLink.EndDateTime = InstantExtensions.GetEndDefault().ToDateTimeUtc();

            chargeLink.Charge.ChargePeriods.Clear();
            chargeLink.Charge.ChargePeriods.Add(GenerateChargePeriod(chargeLink.Charge));

            var chargeLinks = new List<ChargeLink> { chargeLink, }.AsQueryable();

            // Act
            var actual = chargeLinks.AsChargeLinkV2Dto();

            // Assert
            actual.Should().HaveCount(1);
            actual.Should().Contain(c => c.EndDate == null);
        }

        [Theory]
        [InlineAutoMoqData(false, "SecondPeriodName", false)]
        [InlineAutoMoqData(true, "ThirdPeriodName", true)]
        public void AsChargeLinkV2Dto_WhenChargeLinkHasChargeWithManyChargePeriods_UsesCurrentPeriodValues(
            bool includePeriodStartingToday, string expectedName, bool expectedTransparentInvoice, ChargeLink chargeLink)
        {
            // Arrange
            chargeLink.Charge.Type = 1;
            chargeLink.Charge.ChargePeriods.Clear();

            var chargePeriods = GenerateChargePeriods(chargeLink.Charge, includePeriodStartingToday);
            var chargeLinks = BuildChargeLinksWithChargesPeriods(chargeLink, chargePeriods);

            // Act
            var actual = chargeLinks.AsChargeLinkV2Dto();

            // Assert
            actual.Single().ChargeName.Should().Be(expectedName);
            actual.Single().TransparentInvoicing.Should().Be(expectedTransparentInvoice);
        }

        [Theory]
        [InlineAutoMoqData("FirstPeriodName", true)]
        public void AsChargeLinkV2Dto_WhenChargeLinkHasChargeWithOnlyFutureChargePeriods_UsesFirstPeriodValues(
            string expectedName, bool expectedTransparentInvoicing, ChargeLink chargeLink)
        {
            // Arrange
            chargeLink.Charge.Type = 1;
            chargeLink.Charge.ChargePeriods.Clear();

            var chargePeriods = GenerateFutureChargePeriods(chargeLink.Charge);
            var chargeLinks = BuildChargeLinksWithChargesPeriods(chargeLink, chargePeriods);

            // Act
            var actual = chargeLinks.AsChargeLinkV2Dto();

            // Assert
            actual.Single().ChargeName.Should().Be(expectedName);
            actual.Single().TransparentInvoicing.Should().Be(expectedTransparentInvoicing);
        }

        private static ChargePeriod GenerateChargePeriod(Charge charge)
        {
            var period = new ChargePeriodBuilder().Build(charge);
            return period;
        }

        private static IQueryable<ChargeLink> BuildChargeLinksWithChargesPeriods(ChargeLink chargeLink, IEnumerable<ChargePeriod> chargePeriods)
        {
            foreach (var period in chargePeriods)
            {
                chargeLink.Charge.ChargePeriods.Add(period);
            }

            var chargeLinks = new List<ChargeLink> { chargeLink }.AsQueryable();
            return chargeLinks;
        }

        private static IEnumerable<ChargePeriod> GenerateChargePeriods(Charge charge, bool includePeriodStartingToday)
        {
            var chargePeriodBuilder = new ChargePeriodBuilder();
            var today = DateTime.Now.Date.ToUniversalTime();

            if (includePeriodStartingToday)
            {
                var chargePeriods = new List<ChargePeriod>
                {
                    chargePeriodBuilder.WithName("FirstPeriodName").WithTransparentInvoicing(true).WithStartDateTime(today.AddDays(-2)).WithEndDateTime(today.AddDays(-1)).Build(charge),
                    chargePeriodBuilder.WithName("SecondPeriodName").WithTransparentInvoicing(false).WithStartDateTime(today.AddDays(-1)).WithEndDateTime(today).Build(charge),
                    chargePeriodBuilder.WithName("ThirdPeriodName").WithTransparentInvoicing(true).WithStartDateTime(today).WithEndDateTime(today.AddDays(1)).Build(charge),
                    chargePeriodBuilder.WithName("FourthPeriodName").WithTransparentInvoicing(false).WithStartDateTime(today.AddDays(1)).WithEndDateTime(today.AddDays(2)).Build(charge),
                };
                return chargePeriods;
            }
            else
            {
                var chargePeriods = new List<ChargePeriod>
                {
                    chargePeriodBuilder.WithName("FirstPeriodName").WithTransparentInvoicing(true).WithStartDateTime(today.AddDays(-2)).WithEndDateTime(today.AddDays(-1)).Build(charge),
                    chargePeriodBuilder.WithName("SecondPeriodName").WithTransparentInvoicing(false).WithStartDateTime(today.AddDays(-1)).WithEndDateTime(today.AddDays(1)).Build(charge),
                    chargePeriodBuilder.WithName("ThirdPeriodName").WithTransparentInvoicing(true).WithStartDateTime(today.AddDays(1)).WithEndDateTime(today.AddDays(2)).Build(charge),
                };
                return chargePeriods;
            }
        }

        private static IEnumerable<ChargePeriod> GenerateFutureChargePeriods(Charge charge)
        {
            var chargePeriodBuilder = new ChargePeriodBuilder();
            var today = DateTime.Now.Date.ToUniversalTime();

            var chargePeriods = new List<ChargePeriod>
            {
                chargePeriodBuilder.WithName("FirstPeriodName").WithTransparentInvoicing(true).WithStartDateTime(today.AddDays(2)).WithEndDateTime(today.AddDays(4)).Build(charge),
                chargePeriodBuilder.WithName("SecondPeriodName").WithTransparentInvoicing(false).WithStartDateTime(today.AddDays(4)).WithEndDateTime(today.AddDays(6)).Build(charge),
                chargePeriodBuilder.WithName("ThirdPeriodName").WithTransparentInvoicing(true).WithStartDateTime(today.AddDays(6)).WithEndDateTime(today.AddDays(8)).Build(charge),
                chargePeriodBuilder.WithName("FourthPeriodName").WithTransparentInvoicing(false).WithStartDateTime(today.AddDays(8)).WithEndDateTime(today.AddDays(10)).Build(charge),
            };
            return chargePeriods;
        }
    }
}
