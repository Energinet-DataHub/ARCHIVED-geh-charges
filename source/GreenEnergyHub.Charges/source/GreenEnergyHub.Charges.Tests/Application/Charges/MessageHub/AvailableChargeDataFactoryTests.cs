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
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using FluentAssertions;
using GreenEnergyHub.Charges.Application;
using GreenEnergyHub.Charges.Application.Charges.MessageHub;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Tests.Builders;
using GreenEnergyHub.TestHelpers;
using GreenEnergyHub.TestHelpers.FluentAssertionsExtensions;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.Charges.MessageHub
{
    [UnitTest]
    public class AvailableChargeDataFactoryTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task CreateAsync_WhenTaxCharge_CreatesAvailableDataPerActiveGrid(
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<IMessageMetaDataContext> messageMetaDataContext,
            Instant now,
            List<MarketParticipant> marketParticipants,
            ChargeCommandTestBuilder chargeCommandTestBuilder,
            ChargeCommandAcceptedEventTestBuilder chargeCommandAcceptedEventTestBuilder,
            AvailableChargeDataFactory sut)
        {
            // Arrange
            var chargeCommand = chargeCommandTestBuilder.WithPoint(1).WithTaxIndicator(true).Build();
            var acceptedEvent = chargeCommandAcceptedEventTestBuilder.WithChargeCommand(chargeCommand).Build();

            marketParticipantRepository.Setup(
                    r => r.GetActiveGridAccessProvidersAsync())
                .ReturnsAsync(marketParticipants);

            messageMetaDataContext.Setup(
                    m => m.RequestDataTime)
                .Returns(now);

            // Act
            var actualList =
                await sut.CreateAsync(acceptedEvent);

            // Assert
            var operation = acceptedEvent.Command.ChargeOperation;
            actualList.Should().HaveSameCount(marketParticipants);
            for (var i = 0; i < actualList.Count; i++)
            {
                actualList[i].Should().NotContainNullsOrEmptyEnumerables();
                actualList[i].RecipientId.Should().Be(marketParticipants[i].MarketParticipantId);
                actualList[i].RecipientRole.Should().Be(marketParticipants[i].BusinessProcessRole);
                actualList[i].BusinessReasonCode.Should().Be(acceptedEvent.Command.Document.BusinessReasonCode);
                actualList[i].RequestDateTime.Should().Be(now);
                actualList[i].ChargeId.Should().Be(operation.ChargeId);
                actualList[i].ChargeOwner.Should().Be(operation.ChargeOwner);
                actualList[i].ChargeType.Should().Be(operation.Type);
                actualList[i].ChargeName.Should().Be(operation.ChargeName);
                actualList[i].ChargeDescription.Should().Be(operation.ChargeDescription);
                actualList[i].StartDateTime.Should().Be(operation.StartDateTime);
                actualList[i].EndDateTime.Should().Be(operation.EndDateTime.TimeOrEndDefault());
                actualList[i].VatClassification.Should().Be(operation.VatClassification);
                actualList[i].TaxIndicator.Should().Be(operation.TaxIndicator);
                actualList[i].TransparentInvoicing.Should().Be(operation.TransparentInvoicing);
                actualList[i].Resolution.Should().Be(operation.Resolution);
                actualList[i].Points.Should().BeEquivalentTo(
                    operation.Points,
                    options => options.ExcludingMissingMembers());
            }
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task CreateAsync_WhenNotTaxCharge_ReturnsEmptyList(
            ChargeCommandTestBuilder chargeCommandTestBuilder,
            ChargeCommandAcceptedEventTestBuilder chargeCommandAcceptedEventTestBuilder,
            AvailableChargeDataFactory sut)
        {
            // Arrange
            var chargeCommand = chargeCommandTestBuilder.WithTaxIndicator(false).Build();
            var acceptedEvent = chargeCommandAcceptedEventTestBuilder.WithChargeCommand(chargeCommand).Build();

            // Act
            var actualList =
                await sut.CreateAsync(acceptedEvent);

            // Assert
            actualList.Should().BeEmpty();
        }
    }
}
