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

using FluentAssertions;
using GreenEnergyHub.Charges.Domain.AvailableData.Shared;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.TestHelpers;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.MessageHub.Models.Shared
{
    [UnitTest]
    public class AvailableDataFactoryHelperTests
    {
        [Theory]
        [InlineAutoDomainData(MarketParticipantRole.GridAccessProvider, false)]
        [InlineAutoDomainData(MarketParticipantRole.SystemOperator, true)]
        public void ShouldSkipAvailableData_WhenSenderIsSystemOperator_ReturnsTrue(
            MarketParticipantRole role,
            bool expected,
            ChargeLinksCommand chargeLinksCommand)
        {
            chargeLinksCommand.Document.Sender.BusinessProcessRole = role;
            AvailableDataFactoryHelper.ShouldSkipAvailableData(chargeLinksCommand).Should().Be(expected);
        }
    }
}
