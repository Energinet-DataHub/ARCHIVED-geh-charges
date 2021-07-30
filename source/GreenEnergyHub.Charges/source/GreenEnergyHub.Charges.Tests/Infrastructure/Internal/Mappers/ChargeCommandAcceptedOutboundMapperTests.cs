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
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using GreenEnergyHub.Charges.Domain.Events.Local;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeCommandAccepted;
using GreenEnergyHub.Charges.Infrastructure.Internal.Mappers;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.TestHelpers.FluentAssertionsExtensions;
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
        public void Convert_WhenCalled_ShouldMapToProtobufWithCorrectValues([NotNull]ChargeCommand chargeCommand)
        {
            // Arrange
            ChargeCommandAcceptedEvent chargeCommandAcceptedEvent =
                new (SystemClock.Instance.GetCurrentInstant(), chargeCommand.CorrelationId, chargeCommand);

            var mapper = new ChargeCommandAcceptedOutboundMapper();

            UpdateInstantsToValidTimes(chargeCommandAcceptedEvent);

            // Act
            var result = (ChargeCommandAcceptedContract)mapper.Convert(chargeCommandAcceptedEvent);

            // Assert
            AssertExtensions.ContractIsEquivalent(result, chargeCommandAcceptedEvent);
        }

        [Fact]
        public void Convert_WhenCalledWithNull_ShouldThrow()
        {
            var mapper = new ChargeCommandAcceptedOutboundMapper();
            ChargeCommandAcceptedEvent? chargeCommandAcceptedEvent = null;
            Assert.Throws<InvalidOperationException>(() => mapper.Convert(chargeCommandAcceptedEvent!));
        }

        private static void UpdateInstantsToValidTimes([NotNull] ChargeCommandAcceptedEvent chargeCommandAcceptedEvent)
        {
            chargeCommandAcceptedEvent.Command.Document.RequestDate = Instant.FromUtc(2021, 7, 21, 11, 42, 25);
            chargeCommandAcceptedEvent.Command.Document.CreatedDateTime = Instant.FromUtc(2021, 7, 21, 12, 14, 43);
            chargeCommandAcceptedEvent.Command.ChargeOperation.StartDateTime = Instant.FromUtc(2021, 8, 31, 22, 0);
            chargeCommandAcceptedEvent.Command.ChargeOperation.EndDateTime = Instant.FromUtc(2021, 9, 30, 22, 0);

            foreach (var point in chargeCommandAcceptedEvent.Command.ChargeOperation.Points)
            {
                point.Time = SystemClock.Instance.GetCurrentInstant();
            }
        }
    }
}
