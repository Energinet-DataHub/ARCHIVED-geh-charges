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
using AutoFixture.Xunit2;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandAcceptedEvents;

using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeCommandAcceptedEvents
{
    [UnitTest]
    public class ChargeCommandAcceptedEventFactoryTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void CreateEvent_WhenCalled_CreatesEventWithCorrectTime(
            [Frozen] [NotNull] Mock<IClock> clock,
            [NotNull] ChargeInformationCommand command,
            [NotNull] ChargeCommandAcceptedEventFactory sut)
        {
            // Arrange
            var currentTime = Instant.FromUtc(2021, 7, 7, 7, 50, 49);
            clock.Setup(
                    c => c.GetCurrentInstant())
                .Returns(currentTime);

            // Act
            var result = sut.CreateEvent(command);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(currentTime, result.PublishedTime);
        }
    }
}
