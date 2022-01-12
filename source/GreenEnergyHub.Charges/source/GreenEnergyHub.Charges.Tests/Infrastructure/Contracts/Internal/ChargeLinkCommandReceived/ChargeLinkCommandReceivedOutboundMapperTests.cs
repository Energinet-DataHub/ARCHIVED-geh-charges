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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksReceivedEvents;
using GreenEnergyHub.Charges.Infrastructure.Contracts.Internal.ChargeLinksCommandReceived;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Protobuf;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Contracts.Internal.ChargeLinkCommandReceived
{
    [UnitTest]
    public class ChargeLinkCommandReceivedOutboundMapperTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void Convert_WhenCalled_ShouldMapToProtobufWithCorrectValues(
            ChargeLinksCommand chargeLinksCommand,
            ChargeLinksCommandReceivedOutboundMapper sut)
        {
            // Arrange
            ChargeLinksReceivedEvent chargeLinksReceivedEvent =
                new(SystemClock.Instance.GetCurrentInstant(), chargeLinksCommand);

            // Act
            var result = (Charges.Infrastructure.Internal.ChargeLinksCommandReceived.ChargeLinksCommandReceived)sut.Convert(chargeLinksReceivedEvent);

            // Assert
            ProtobufAssert.OutgoingContractIsSubset(chargeLinksReceivedEvent, result);
        }

        [Theory]
        [InlineAutoMoqData]
        public void Convert_WhenCalledWithNull_ShouldThrow(ChargeLinksCommandReceivedOutboundMapper sut)
        {
            Assert.Throws<InvalidOperationException>(() => sut.Convert(null!));
        }
    }
}
