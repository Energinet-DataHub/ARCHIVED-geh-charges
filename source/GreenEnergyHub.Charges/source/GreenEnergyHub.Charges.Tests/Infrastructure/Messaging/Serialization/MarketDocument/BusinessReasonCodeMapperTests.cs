﻿// Copyright 2020 Energinet DataHub A/S
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

using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Infrastructure.Messaging.Serialization.MarketDocument;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Messaging.Serialization.MarketDocument
{
    [UnitTest]
    public class BusinessReasonCodeMapperTests
    {
        [Theory]
        [InlineData("D18", BusinessReasonCode.UpdateChargeInformation)]
        [InlineData("D17", BusinessReasonCode.UpdateMasterDataSettlement)]
        [InlineData("", BusinessReasonCode.Unknown)]
        [InlineData("DoesNotExist", BusinessReasonCode.Unknown)]
        [InlineData(null, BusinessReasonCode.Unknown)]
        public void Map_WhenGivenInput_MapsToCorrectEnum(string unit, BusinessReasonCode expected)
        {
            var actual = BusinessReasonCodeMapper.Map(unit);
            Assert.Equal(actual, expected);
        }
    }
}
