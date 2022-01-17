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
using GreenEnergyHub.Charges.Domain.HubSenderMarketParticipant;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Xunit;

namespace GreenEnergyHub.Charges.Tests.Domain.HubSenderMarketParticipants
{
    public class HubSenderMarketParticipantTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void Ctor_WhenNoRoles_ThrowsArgumentException(
            Guid anyGuid,
            string anyMarketParticipantId,
            List<MarketParticipantRole> emptyRoles)
        {
            emptyRoles.Clear();
            Assert.Throws<ArgumentException>(
                () => new HubSenderMarketParticipant(anyGuid, anyMarketParticipantId, true, emptyRoles));
        }

        [Theory]
        [InlineAutoMoqData]
        public void Ctor_WhenNoMeteringPointAdministratorRole_ThrowsArgumentException(
            Guid anyGuid,
            string anyMarketParticipantId)
        {
            var roles = new List<MarketParticipantRole>
            {
                MarketParticipantRole.EnergySupplier,
                MarketParticipantRole.SystemOperator,
            };

            Assert.Throws<ArgumentException>(
                () => new HubSenderMarketParticipant(anyGuid, anyMarketParticipantId, true, roles));
        }
    }
}
