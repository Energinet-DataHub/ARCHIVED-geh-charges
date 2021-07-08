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

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using GreenEnergyHub.Charges.Application;
using GreenEnergyHub.Charges.Application.Acknowledgement;
using GreenEnergyHub.Charges.Domain.Acknowledgements;
using GreenEnergyHub.Charges.Domain.Events.Local;
using GreenEnergyHub.Charges.TestCore;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.Acknowledgement
{
    [UnitTest]
    public class ChargeRejectionSenderTests
    {
        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_WhenCalled_DispatchRejection(
            [Frozen] [NotNull] Mock<IMessageDispatcher<ChargeRejection>> dispatcher,
            [NotNull] ChargeCommandRejectedEvent acceptedEvent,
            [NotNull] ChargeRejectionSender sut)
        {
            // Arrange
            var dispatched = false;
            dispatcher.Setup(
                    d => d.DispatchAsync(
                        It.IsAny<ChargeRejection>(),
                        It.IsAny<CancellationToken>()))
                .Callback<ChargeRejection, CancellationToken>(
                    (_, _) => dispatched = true);

            // Act
            await sut.HandleAsync(acceptedEvent).ConfigureAwait(false);

            // Assert
            Assert.True(dispatched);
        }
    }
}
