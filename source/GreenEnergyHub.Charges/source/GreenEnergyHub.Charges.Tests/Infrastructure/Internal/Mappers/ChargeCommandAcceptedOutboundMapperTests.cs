﻿// Copyright 2020 Energinet DataHub A/S
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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandAcceptedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeCommandAccepted;
using GreenEnergyHub.Charges.Infrastructure.Internal.Mappers;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Protobuf;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Internal.Mappers
{
    [UnitTest]
    public class ChargeCommandAcceptedOutboundMapperTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void Convert_WhenCalled_ShouldMapToProtobufWithCorrectValues(
            ChargeCommand chargeCommand,
            ChargeCommandAcceptedOutboundMapper sut)
        {
            // Arrange
            ChargeCommandAcceptedEvent chargeCommandAcceptedEvent =
                new(SystemClock.Instance.GetCurrentInstant(), chargeCommand);

            UpdateInstantsToValidTimes(chargeCommandAcceptedEvent);

            // Act
            var result = (ChargeCommandAcceptedContract)sut.Convert(chargeCommandAcceptedEvent);

            // Assert
            ProtobufAssert.OutgoingContractIsSubset(chargeCommandAcceptedEvent, result);
        }

        [Theory]
        [InlineAutoMoqData]
        public void Convert_WhenCalledWithNull_ShouldThrow(ChargeCommandAcceptedOutboundMapper sut)
        {
            Assert.Throws<InvalidOperationException>(() => sut.Convert(null!));
        }

        private static void UpdateInstantsToValidTimes(ChargeCommandAcceptedEvent chargeCommandAcceptedEvent)
        {
            chargeCommandAcceptedEvent.Command.Document.RequestDate = Instant.FromUtc(2021, 7, 21, 11, 42, 25);
            chargeCommandAcceptedEvent.Command.Document.CreatedDateTime = Instant.FromUtc(2021, 7, 21, 12, 14, 43);
        }
    }
}
