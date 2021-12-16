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
using Energinet.DataHub.Charges.Clients.DefaultChargeLink.Mappers;
using FluentAssertions;
using Xunit;
using Xunit.Categories;
using CreateDefaultChargeLinksFailed =
    Energinet.Charges.Contracts.CreateDefaultChargeLinksReply.Types.CreateDefaultChargeLinksFailed;

namespace Energinet.DataHub.Charges.Clients.CreateDefaultChargeLink.Tests.DefaultChargeLink.Mappers
{
    [UnitTest]
    public class CreateDefaultChargeLinksFailedInboundMapperTests
    {
        private readonly Fixture _fixture;

        public CreateDefaultChargeLinksFailedInboundMapperTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public void Convert_WhenCalled_MapsToDtoWithCorrectValues()
        {
            // Arrange
            var createDefaultChargeLinksReply = _fixture.Create<CreateDefaultChargeLinksReply>();
            var createDefaultChargeLinksFailed = _fixture.Create<CreateDefaultChargeLinksFailed>();
            createDefaultChargeLinksReply.CreateDefaultChargeLinksFailed = createDefaultChargeLinksFailed;

            // Act
            var (meteringPointId, errorCode) = CreateDefaultChargeLinksFailedInboundMapper.Convert(createDefaultChargeLinksReply);

            // Assert
            meteringPointId.Should().Be(createDefaultChargeLinksReply.MeteringPointId);
            ((int)errorCode).Should().Be((int)createDefaultChargeLinksFailed.ErrorCode);
        }

        [Fact]
        public void Convert_WhenCalledWithNull_ThrowException()
        {
            Assert.Throws<NullReferenceException>(() => CreateDefaultChargeLinksFailedInboundMapper.Convert(null!));
        }
    }
}
