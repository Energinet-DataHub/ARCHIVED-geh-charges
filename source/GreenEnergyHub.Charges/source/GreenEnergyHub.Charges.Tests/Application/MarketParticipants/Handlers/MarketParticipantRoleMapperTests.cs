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
using GreenEnergyHub.Charges.Application.MarketParticipants.Handlers;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.TestHelpers;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.MarketParticipants.Handlers
{
    [UnitTest]
    public class MarketParticipantRoleMapperTests
    {
        [Theory]
        [InlineAutoDomainData(BusinessRoleCode.Ddm, MarketParticipantRole.GridAccessProvider)]
        [InlineAutoDomainData(BusinessRoleCode.Ddq, MarketParticipantRole.EnergySupplier)]
        [InlineAutoDomainData(BusinessRoleCode.Ddz, MarketParticipantRole.MeteringPointAdministrator)]
        [InlineAutoDomainData(BusinessRoleCode.Ez, MarketParticipantRole.SystemOperator)]
        [InlineAutoDomainData(BusinessRoleCode.Ddk, MarketParticipantRole.BalanceResponsibleParty)]
        [InlineAutoDomainData(BusinessRoleCode.Ddx, MarketParticipantRole.ImbalanceSettlementResponsible)]
        [InlineAutoDomainData(BusinessRoleCode.Dgl, MarketParticipantRole.MeteredDataAdministrator)]
        [InlineAutoDomainData(BusinessRoleCode.Mdr, MarketParticipantRole.MeteredDataResponsible)]
        [InlineAutoDomainData(BusinessRoleCode.Sts, MarketParticipantRole.EnergyAgency)]
        [InlineAutoDomainData(BusinessRoleCode.Tso, MarketParticipantRole.TransmissionSystemOperator)]
        public void Map_WhenValidBusinessRoleCode_ThenMapsToMarketParticipantRole(
            BusinessRoleCode businessRoleCode,
            MarketParticipantRole marketParticipantRole)
        {
            MarketParticipantRoleMapper.Map(businessRoleCode).Should().Be(marketParticipantRole);
        }

        [Theory]
        [InlineAutoDomainData(-1)]
        public void Map_WhenInValidBusinessRoleCode_ThenThrowsArgumentOutOfRangeException(
            BusinessRoleCode businessRoleCode)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => MarketParticipantRoleMapper.Map(businessRoleCode));
        }
    }
}
