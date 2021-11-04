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
using CreateDefaultChargeLinkMessagesSucceeded =
    Energinet.Charges.Contracts.CreateDefaultChargeLinkMessagesReply.Types.CreateDefaultChargeLinkMessagesSucceeded;

namespace Energinet.DataHub.Charges.Clients.CreateDefaultChargeLink.Tests.DefaultChargeLinkMessages.Mappers
{
    [UnitTest]
    public class CreateDefaultChargeLinkMessagesSucceededInboundMapperTests
    {
        private readonly Fixture _fixture;

        public CreateDefaultChargeLinkMessagesSucceededInboundMapperTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public void Convert_WhenCalled_MapsToDtoWithCorrectValues()
        {
            // Arrange
            var createDefaultChargeLinkMessagesReply = _fixture.Create<CreateDefaultChargeLinkMessagesReply>();
            var createDefaultChargeLinkMessagesSucceeded = _fixture.Create<CreateDefaultChargeLinkMessagesSucceeded>();
            createDefaultChargeLinkMessagesReply.CreateDefaultChargeLinkMessagesSucceeded = createDefaultChargeLinkMessagesSucceeded;

            // Act
            var result = CreateDefaultChargeLinkMessagesSucceededInboundMapper.Convert(createDefaultChargeLinkMessagesReply);

            // Assert
            result.MeteringPointId.Should().Be(createDefaultChargeLinkMessagesReply.MeteringPointId);
        }

        [Fact]
        public void Convert_WhenCalledWithNull_ThrowsException()
        {
            Assert.Throws<NullReferenceException>(() => CreateDefaultChargeLinksSucceededInboundMapper.Convert(null!));
        }
    }
}
