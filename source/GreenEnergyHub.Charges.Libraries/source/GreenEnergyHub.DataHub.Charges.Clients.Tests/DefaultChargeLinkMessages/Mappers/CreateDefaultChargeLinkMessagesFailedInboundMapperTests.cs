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
using FluentAssertions;
using GreenEnergyHub.Charges.Contracts;
using GreenEnergyHub.DataHub.Charges.Libraries.Protobuf;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.DataHub.Charges.Clients.CreateDefaultChargeLink.Tests.DefaultChargeLinkMessages.Mappers
{
    [UnitTest]
    public class CreateDefaultChargeLinkMessagesFailedInboundMapperTests
    {
        private readonly Fixture _fixture;

        public CreateDefaultChargeLinkMessagesFailedInboundMapperTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public void Convert_WhenCalled_ShouldMapToDtoWithCorrectValues()
        {
            // Arrange
            var createDefaultChargeLinksFailed = _fixture.Create<CreateDefaultChargeLinksFailed>();

            // Act
            var (meteringPointId, errorCode) = CreateDefaultChargeLinksFailedInboundMapper.Convert(createDefaultChargeLinksFailed);

            // Assert
            meteringPointId.Should().Be(createDefaultChargeLinksFailed.MeteringPointId);
            ((int)errorCode).Should().Be((int)createDefaultChargeLinksFailed.ErrorCode);
        }

        [Fact]
        public void Convert_WhenCalledWithNull_ShouldThrow()
        {
            Assert.Throws<ArgumentNullException>(() => CreateDefaultChargeLinksFailedInboundMapper.Convert(null!));
        }
    }
}
