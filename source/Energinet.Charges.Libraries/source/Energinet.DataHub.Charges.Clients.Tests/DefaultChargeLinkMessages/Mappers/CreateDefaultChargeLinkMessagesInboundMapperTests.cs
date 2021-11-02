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
using AutoFixture;
using Energinet.Charges.Contracts;
using Energinet.DataHub.Charges.Libraries.Protobuf;
using FluentAssertions;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Charges.Clients.CreateDefaultChargeLink.Tests.DefaultChargeLinkMessages.Mappers
{
    [UnitTest]
    public class CreateDefaultChargeLinkMessagesInboundMapperTests
    {
        private readonly Fixture _fixture;

        public CreateDefaultChargeLinkMessagesInboundMapperTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public void Convert_WhenCalled_ShouldMapToDtoWithCorrectValues()
        {
            // Arrange
            var createDefaultChargeLinkMessages = _fixture.Create<CreateDefaultChargeLinkMessages>();

            // Act
            var result = CreateDefaultChargeLinkMessagesInboundMapper.Convert(createDefaultChargeLinkMessages);

            // Assert
            result.MeteringPointId.Should().Be(createDefaultChargeLinkMessages.MeteringPointId);
        }

        [Fact]
        public void Convert_WhenCalledWithNull_ShouldThrow()
        {
            Assert.Throws<NullReferenceException>(() => CreateDefaultChargeLinkMessagesInboundMapper.Convert(null!));
        }
    }
}
