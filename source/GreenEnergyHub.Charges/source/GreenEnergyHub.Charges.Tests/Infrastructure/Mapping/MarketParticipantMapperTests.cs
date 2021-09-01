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
using GreenEnergyHub.Charges.Domain.MarketDocument;
using GreenEnergyHub.Charges.Infrastructure.Context.Mapping;
using GreenEnergyHub.Charges.Infrastructure.Context.Model;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Microsoft.Azure.Amqp.Serialization;
using Xunit;
using Xunit.Categories;
using MarketParticipant = GreenEnergyHub.Charges.Infrastructure.Context.Model.MarketParticipant;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Mapping
{
    [UnitTest]
    public class MarketParticipantMapperTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void ToDomainObject_WhenGivenParticipants_ReturnsMappedObject(
            [NotNull] MarketParticipant marketParticipant,
            [NotNull] MarketParticipantMapper sut)
        {
            // Act
            var result = sut.ToDomainObject(marketParticipant);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(marketParticipant.MarketParticipantId, result.Id);
            Assert.Equal(marketParticipant.Role, (int)result.BusinessProcessRole);
        }

        [Theory]
        [InlineAutoMoqData]
        public void ToDomainObject_IfParticipantIsNull_ThrowsArgumentNullException(
            [NotNull] MarketParticipantMapper sut)
        {
            // Arrange
            MarketParticipant? marketParticipant = null;

            // Act / Assert
            Assert.Throws<ArgumentNullException>(
                () => sut.ToDomainObject(marketParticipant!));
        }
    }
}
