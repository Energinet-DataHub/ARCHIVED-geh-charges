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
using Energinet.DataHub.Charges.Libraries.Models;
using Energinet.DataHub.Charges.Libraries.Protobuf;
using FluentAssertions;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Charges.Clients.CreateDefaultChargeLink.Tests.DefaultChargeLinkMessages.Mappers
{
    [UnitTest]
    public class CreateDefaultChargeLinkMessagesSucceededOutboundMapperTests
    {
        private readonly Fixture _fixture;

        public CreateDefaultChargeLinkMessagesSucceededOutboundMapperTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public void Convert_WhenCalled_ShouldMapToProtobufWithCorrectValues()
        {
            // Arrange
            var createDefaultChargeLinksSucceededDto = _fixture.Create<CreateDefaultChargeLinksSucceededDto>();

            // Act
            var actual = CreateDefaultChargeLinksSucceededOutboundMapper.Convert(createDefaultChargeLinksSucceededDto);

            // Assert
            actual.MeteringPointId.Should().Be(createDefaultChargeLinksSucceededDto.MeteringPointId);
            actual.CreateDefaultChargeLinksSucceeded.DidCreateChargeLinks
                .Should().Be(createDefaultChargeLinksSucceededDto.DidCreateChargeLinks);
        }

        [Fact]
        public void Convert_WhenCalledWithNull_ShouldThrow()
        {
            Assert.Throws<NullReferenceException>(() => CreateDefaultChargeLinksSucceededOutboundMapper.Convert(null!));
        }
    }
}
