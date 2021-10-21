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
using GreenEnergyHub.Charges.Domain.AvailableChargeData;
using GreenEnergyHub.Charges.Domain.AvailableChargeLinksData;
using GreenEnergyHub.Charges.Domain.ChargeCommands;
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
    public class AvailableChargeDataFactoryTests
    {
        [Theory]
        [InlineAutoDomainData]
        public void CreateFromCommandAsync_Charge_HasNoNullsOrEmptyCollections(
            [NotNull] ChargeCommand chargeCommand,
            [NotNull] Instant now,
            [NotNull] Guid messageHubId,
            [NotNull] AvailableChargeDataFactory sut)
        {
            // Act
            var actual =
                sut.Create(chargeCommand, now, messageHubId);

            // Assert
            actual.Should().NotContainNullsOrEmptyEnumerables();
            actual.RequestTime.Should().Be(now);
            actual.AvailableDataReferenceId.Should().Be(messageHubId);
        }
    }
}
