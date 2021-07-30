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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using GreenEnergyHub.Charges.Domain.Events.Local;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeCommandRejected;
using GreenEnergyHub.Charges.Infrastructure.Internal.Mappers;
using GreenEnergyHub.Charges.TestCore;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Internal.Mappers
{
    [UnitTest]
    public class ChargeCommandRejectedOutboundMapperTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void Convert_WhenCalled_ShouldMapToProtobufWithCorrectValues(
            [NotNull]ChargeCommand chargeCommand,
            [NotNull] ChargeCommandRejectedOutboundMapper sut)
        {
            var chargeCommandRejectedEvent = CreateChargeCommandRejectedEvent(chargeCommand);
            var result = (ChargeCommandRejectedContract)sut.Convert(chargeCommandRejectedEvent);
            AssertExtensions.ContractIsEquivalent(result, chargeCommandRejectedEvent);
        }

        [Theory]
        [InlineAutoMoqData]
        public void Convert_WhenCalledWithNull_ShouldThrow([NotNull] ChargeCommandRejectedOutboundMapper sut)
        {
            Assert.Throws<InvalidOperationException>(() => sut.Convert(null!));
        }

        private static ChargeCommandRejectedEvent CreateChargeCommandRejectedEvent(ChargeCommand chargeCommand)
        {
            var reasons = new List<string> { "reason 1", "reason 2" };
            var chargeCommandRejectedEvent = new ChargeCommandRejectedEvent(
                SystemClock.Instance.GetCurrentInstant(),
                chargeCommand.CorrelationId,
                chargeCommand,
                reasons);
            UpdateInstantsToValidTimes(chargeCommandRejectedEvent);
            return chargeCommandRejectedEvent;
        }

        private static void UpdateInstantsToValidTimes([NotNull] ChargeCommandRejectedEvent chargeCommandRejectedEvent)
        {
            chargeCommandRejectedEvent.Command.Document.RequestDate = Instant.FromUtc(2021, 7, 21, 11, 42, 25);
            chargeCommandRejectedEvent.Command.Document.CreatedDateTime = Instant.FromUtc(2021, 7, 21, 12, 14, 43);
            chargeCommandRejectedEvent.Command.ChargeOperation.StartDateTime = Instant.FromUtc(2021, 8, 31, 22, 0);
            chargeCommandRejectedEvent.Command.ChargeOperation.EndDateTime = Instant.FromUtc(2021, 9, 30, 22, 0);

            foreach (var point in chargeCommandRejectedEvent.Command.ChargeOperation.Points)
            {
                point.Time = SystemClock.Instance.GetCurrentInstant();
            }
        }
    }
}
