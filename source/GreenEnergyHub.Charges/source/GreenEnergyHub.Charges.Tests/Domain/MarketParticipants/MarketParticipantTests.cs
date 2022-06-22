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
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using Xunit;

namespace GreenEnergyHub.Charges.Tests.Domain.MarketParticipants
{
    public class MarketParticipantTests
    {
        [Theory]
        [MemberData(nameof(InvalidBusinessProcessRoles))]
        public void Ctor_WhenInvalidBusinessProcessRole_ThrowsArgumentException(MarketParticipantRole invalidRole)
        {
            Assert.Throws<ArgumentException>(() =>
                new MarketParticipant(Guid.NewGuid(), Guid.NewGuid(), string.Empty, false, invalidRole));
        }

        [Theory]
        [MemberData(nameof(ValidBusinessProcessRoles))]
        public void Ctor_SetsRole(MarketParticipantRole role)
        {
            var actual = new MarketParticipant(Guid.NewGuid(), Guid.NewGuid(), string.Empty, false, role);
            actual.BusinessProcessRole.Should().Be(role);
        }

        public static IEnumerable<object[]> ValidBusinessProcessRoles =>
            DomainBusinessProcessRoles.Select(r => new object[] { r });

        public static IEnumerable<object[]> InvalidBusinessProcessRoles => Enum
            .GetValues<MarketParticipantRole>()
            .Except(DomainBusinessProcessRoles)
            .Select(r => new object[] { r });

        private static List<MarketParticipantRole> DomainBusinessProcessRoles => new List<MarketParticipantRole>
        {
            MarketParticipantRole.EnergySupplier,
            MarketParticipantRole.SystemOperator,
            MarketParticipantRole.GridAccessProvider,
            MarketParticipantRole.MeteringPointAdministrator,
        };
    }
}
