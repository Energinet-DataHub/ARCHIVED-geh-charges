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

using System.Threading.Tasks;
using FluentAssertions;
using GreenEnergyHub.Charges.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.WebApi.Controllers
{
    [UnitTest]
    public class ChargeLinksControllerTests
    {
        [Fact]
        public async Task GetChargeLinksByMeteringPointIdAsync_WithMeteringPointId_ReturnsOk()
        {
            var controller = new ChargeLinksController();

            var result = await controller.GetAsync("570000000000000000").ConfigureAwait(false);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetChargeLinksByMeteringPointIdAsync_WhenRequesting404_ReturnsNotFound()
        {
            var controller = new ChargeLinksController();

            var result = await controller.GetAsync("404").ConfigureAwait(false);

            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task GetChargeLinksByMeteringPointIdAsync_WhenNoInputGiven_ReturnsBadRequest()
        {
            var controller = new ChargeLinksController();

            var result = await controller.GetAsync(null).ConfigureAwait(false);

            result.Should().BeOfType<BadRequestResult>();
        }
    }
}
