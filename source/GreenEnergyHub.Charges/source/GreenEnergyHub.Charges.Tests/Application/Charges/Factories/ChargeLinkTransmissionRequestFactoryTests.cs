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
using GreenEnergyHub.Charges.Domain.ChargeLinkCommandAcceptedEvents;
using GreenEnergyHub.Charges.Domain.ChargeLinkTransmissionRequest;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.TestHelpers;
using GreenEnergyHub.TestHelpers.FluentAssertionsExtensions;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.Charges.Factories
{
    [UnitTest]
    public class ChargeLinkTransmissionRequestFactoryTests
    {
        [Theory]
        [InlineAutoDomainData]
        public void CreateFromCommandAsync_Charge_HasNoNullsOrEmptyCollections(
            [NotNull] ChargeLinkCommandAcceptedEvent chargeLinkCommandAcceptedEvent,
            [NotNull] MarketParticipant marketParticipant,
            [NotNull] Guid messageHubId,
            [NotNull] ChargeLinkTransmissionRequestFactory sut)
        {
            // Act
            var actual =
                sut.MapChargeLinkCommandAcceptedEvent(chargeLinkCommandAcceptedEvent, marketParticipant, messageHubId);

            // Assert
            actual.Should().NotContainNullsOrEmptyEnumerables();
            actual.Recipient.Should().Be(marketParticipant.Id);
            actual.RecipientRole.Should().Be(marketParticipant.BusinessProcessRole);
            actual.BusinessReasonCode.Should().Be(chargeLinkCommandAcceptedEvent.Document.BusinessReasonCode);
            actual.ChargeId.Should().Be(chargeLinkCommandAcceptedEvent.ChargeLink.SenderProvidedChargeId);
            actual.ChargeOwner.Should().Be(chargeLinkCommandAcceptedEvent.Document.Sender.Id);
            actual.ChargeType.Should().Be(chargeLinkCommandAcceptedEvent.ChargeLink.ChargeType);
            actual.MeteringPointId.Should().Be(chargeLinkCommandAcceptedEvent.ChargeLink.MeteringPointId);
            actual.Factor.Should().Be(chargeLinkCommandAcceptedEvent.ChargeLink.Factor);
            actual.StartDateTime.Should().Be(chargeLinkCommandAcceptedEvent.ChargeLink.StartDateTime);
            actual.EndDateTime.Should().Be(chargeLinkCommandAcceptedEvent.ChargeLink.EndDateTime.GetValueOrDefault());
            actual.MessageHubId.Should().Be(messageHubId);
        }
    }
}
