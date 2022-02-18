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
using Azure.Messaging.EventHubs.Producer;
using Energinet.Charges.Contracts.Charge;
using Energinet.Charges.Contracts.ChargeLink;
using FluentAssertions;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.QueryApi.Model;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.WebApi.ModelPredicates;
using Microsoft.OData.Edm;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.WebApi.ModelPredicates
{
    [UnitTest]
    public class ChargeLinkQueryLogicTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void AsChargeLinkV1Dto_WhenChargeLinkContainsEndDefaultEndDate_EndDateReturnsAsNull(ChargeLink chargeLink)
        {
            // Arrange
            chargeLink.Charge.Type = 1;
            chargeLink.EndDateTime = InstantExtensions.GetEndDefault().ToDateTimeUtc();
            var chargeLinks = new List<ChargeLink> { chargeLink, }.AsQueryable();

            // Act
            var actual = chargeLinks.AsChargeLinkV1Dto();

            // Assert
            actual.Should().NotContain(c => c.EndDate == InstantExtensions.GetEndDefault().ToDateTimeUtc());
        }

        [Theory]
        [InlineAutoMoqData]
        public void AsChargeLinkV1Dto_WhenChargeLinkHasChargeWithManyChargePeriods_UsesCurrentPeriodValues(ChargeLink chargeLink)
        {
            // Arrange
            chargeLink.Charge.Type = 1;
            chargeLink.Charge.ChargePeriods.Clear();

            var chargePeriods = CreateListWithChargePeriodsForBoundaryTests(chargeLink.Charge);

            foreach (var period in chargePeriods)
            {
                chargeLink.Charge.ChargePeriods.Add(period);
            }

            var chargeLinks = new List<ChargeLink> { chargeLink }.AsQueryable();

            // Act
            var actual = chargeLinks.AsChargeLinkV1Dto();

            // Assert
            actual.Single().ChargeName.Should().Be("ThirdName");
        }

        [Theory]
        [InlineAutoMoqData]
        public void AsChargeLinkV2Dto_SetsAllProperties(ChargeLink chargeLink)
        {
            // Arrange
            chargeLink.Charge.Type = 1;
            chargeLink.Charge.OwnerId = chargeLink.Charge.Owner.Id;
            var chargeLinks = new List<ChargeLink> { chargeLink, }.AsQueryable();
            var expected = new ChargeLinkV2Dto(
                (ChargeType)chargeLink.Charge.Type,
                chargeLink.Charge.SenderProvidedChargeId,
                chargeLink.Charge.ChargePeriods.Where(cp => cp.StartDateTime <= DateTime.UtcNow.Date).OrderByDescending(cp => cp.StartDateTime).First().Name,
                chargeLink.Charge.OwnerId,
                chargeLink.Charge.TaxIndicator,
                chargeLink.Charge.ChargePeriods.Where(cp => cp.StartDateTime <= DateTime.UtcNow.Date).OrderByDescending(cp => cp.StartDateTime).First().TransparentInvoicing,
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
            var chargeLinks = new List<ChargeLink> { chargeLink, }.AsQueryable();

            // Act
            var actual = chargeLinks.AsChargeLinkV2Dto();

            // Assert
            actual.Should().NotContain(c => c.EndDate == InstantExtensions.GetEndDefault().ToDateTimeUtc());
        }

        [Theory]
        [InlineAutoMoqData]
        public void AsChargeLinkV2Dto_WhenChargeLinkHasChargeWithManyChargePeriods_UsesCurrentPeriodValues(ChargeLink chargeLink)
        {
            // Arrange
            chargeLink.Charge.Type = 1;
            chargeLink.Charge.ChargePeriods.Clear();

            var chargePeriods = CreateListWithChargePeriodsForBoundaryTests(chargeLink.Charge);

            foreach (var period in chargePeriods)
            {
                chargeLink.Charge.ChargePeriods.Add(period);
            }

            var chargeLinks = new List<ChargeLink> { chargeLink }.AsQueryable();

            // Act
            var actual = chargeLinks.AsChargeLinkV2Dto();

            // Assert
            actual.Single().ChargeName.Should().Be("ThirdName");
        }

        private static IEnumerable<ChargePeriod> CreateListWithChargePeriodsForBoundaryTests(Charge charge)
        {
            var today = DateTime.UtcNow.Date;

            var chargePeriods = new List<ChargePeriod>
            {
                GenerateChargePeriod(today.AddDays(-2), today.AddDays(-1), charge, "FirstName", true),
                GenerateChargePeriod(today.AddDays(-1), today.AddDays(0), charge, "SecondName", false),
                GenerateChargePeriod(today.AddDays(0), today.AddDays(1), charge, "ThirdName", true),
                GenerateChargePeriod(today.AddDays(1), today.AddDays(2), charge, "FourthName", false),
            };

            return chargePeriods;
        }

        private static ChargePeriod GenerateChargePeriod(DateTime periodStart, DateTime periodEnd, Charge charge, string name, bool transparentInvoicing)
        {
            return new ChargePeriod
            {
                Id = Guid.NewGuid(),
                Charge = charge,
                ChargeId = charge.Id,
                TransparentInvoicing = false,
                Description = "Description",
                Name = name,
                VatClassification = 0,
                StartDateTime = periodStart,
                EndDateTime = periodEnd,
            };
        }
    }
}
