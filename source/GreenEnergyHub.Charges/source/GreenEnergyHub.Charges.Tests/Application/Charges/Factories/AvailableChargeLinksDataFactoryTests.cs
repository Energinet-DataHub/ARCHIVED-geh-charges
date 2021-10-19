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
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.AvailableChargeLinksData;
using GreenEnergyHub.Charges.Domain.ChargeLinkCommandAcceptedEvents;
using GreenEnergyHub.Charges.Domain.ChargeLinkCommands;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.TestHelpers;
using GreenEnergyHub.TestHelpers.FluentAssertionsExtensions;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.Charges.Factories
{
    [UnitTest]
    public class AvailableChargeLinksDataFactoryTests
    {
        [Theory]
        [InlineAutoDomainData]
        public void CreateFromCommandAsync_Charge_HasNoNullsOrEmptyCollections(
            [NotNull] ChargeLinkCommand chargeLinkCommand,
            [NotNull] MarketParticipant marketParticipant,
            [NotNull] Instant now,
            [NotNull] Guid messageHubId,
            [NotNull] AvailableChargeLinksDataFactory sut)
        {
            // Act
            var actual =
                sut.CreateAvailableChargeLinksData(chargeLinkCommand, marketParticipant, now, messageHubId);

            // Assert
            actual.Should().NotContainNullsOrEmptyEnumerables();
            actual.RecipientId.Should().Be(marketParticipant.Id);
            actual.RecipientRole.Should().Be(marketParticipant.BusinessProcessRole);
            actual.BusinessReasonCode.Should().Be(chargeLinkCommand.Document.BusinessReasonCode);
            actual.ChargeId.Should().Be(chargeLinkCommand.ChargeLink.SenderProvidedChargeId);
            actual.ChargeOwner.Should().Be(chargeLinkCommand.Document.Sender.Id);
            actual.ChargeType.Should().Be(chargeLinkCommand.ChargeLink.ChargeType);
            actual.MeteringPointId.Should().Be(chargeLinkCommand.ChargeLink.MeteringPointId);
            actual.Factor.Should().Be(chargeLinkCommand.ChargeLink.Factor);
            actual.StartDateTime.Should().Be(chargeLinkCommand.ChargeLink.StartDateTime);
            actual.EndDateTime.Should().Be(chargeLinkCommand.ChargeLink.EndDateTime.GetValueOrDefault());
            actual.RequestTime.Should().Be(now);
            actual.AvailableDataReferenceId.Should().Be(messageHubId);
        }
    }
}
