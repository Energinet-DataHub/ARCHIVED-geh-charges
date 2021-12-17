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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using FluentAssertions;
using GreenEnergyHub.Charges.QueryApi;
using GreenEnergyHub.Charges.QueryApi.Model;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.WebApi.Controllers
{
    [UnitTest]
    public class ChargeLinksControllerTests
    {
        [Theory]
        [InlineAutoMoqData]
        public async Task GetChargeLinksByMeteringPointIdAsync_WithMeteringPointId_ReturnsOk(
            [Frozen] Mock<IData> data)
        {
            // Arrange
            data.Setup(d => d.Charges).Returns(new List<Charge>().AsQueryable());
            var sut = new ChargeLinksController(data.Object);

            // Act
            var result = await sut.GetAsync("570000000000000000").ConfigureAwait(false);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task GetChargeLinksByMeteringPointIdAsync_WhenRequesting404_ReturnsNotFound(
            [Frozen] Mock<IData> data)
        {
            // Arrange
            data.Setup(d => d.Charges).Returns(new List<Charge>().AsQueryable());
            var sut = new ChargeLinksController(data.Object);

            // Act
            var result = await sut.GetAsync("404").ConfigureAwait(false);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task GetChargeLinksByMeteringPointIdAsync_WhenNoInputGiven_ReturnsBadRequest(
            [Frozen] Mock<IData> data)
        {
            // Arrange
            data.Setup(d => d.Charges).Returns(new List<Charge>().AsQueryable());
            var sut = new ChargeLinksController(data.Object);

            // Act
            var result = await sut.GetAsync(null).ConfigureAwait(false);

            // Assert
            result.Should().BeOfType<BadRequestResult>();
        }
    }
}
