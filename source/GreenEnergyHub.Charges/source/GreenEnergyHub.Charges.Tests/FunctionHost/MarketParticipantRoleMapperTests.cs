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
using Energinet.DataHub.MarketParticipant.Integration.Model.Dtos;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.FunctionHost.MarketParticipant;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.FunctionHost
{
    [UnitTest]
    public class MarketParticipantRoleMapperTests
    {
        [Fact]
        public void When_ActorIntegrationEvent_Received_Then_Map_BusinessRole_To_MarketParticipantRole()
        {
            MarketParticipantRoleMapper.Map(BusinessRoleCode.Ddm).Should().Be(MarketParticipantRole.GridAccessProvider);
            MarketParticipantRoleMapper.Map(BusinessRoleCode.Ddq).Should().Be(MarketParticipantRole.EnergySupplier);
            MarketParticipantRoleMapper.Map(BusinessRoleCode.Ddz).Should().Be(MarketParticipantRole.MeteringPointAdministrator);
            MarketParticipantRoleMapper.Map(BusinessRoleCode.Ez).Should().Be(MarketParticipantRole.SystemOperator);
            Assert.Throws<NotImplementedException>(() => MarketParticipantRoleMapper.Map(BusinessRoleCode.Ddx));
        }
    }
}
