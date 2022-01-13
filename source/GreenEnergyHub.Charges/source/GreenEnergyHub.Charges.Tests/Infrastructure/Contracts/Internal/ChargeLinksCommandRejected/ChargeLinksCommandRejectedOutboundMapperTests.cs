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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksRejectionEvents;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Infrastructure.Contracts.Internal.ChargeLinksCommandRejected;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Builders;
using GreenEnergyHub.Charges.Tests.Protobuf;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Contracts.Internal.ChargeLinksCommandRejected
{
    [UnitTest]
    public class ChargeLinksCommandRejectedOutboundMapperTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void Convert_WhenCalled_ShouldMapToProtobufWithCorrectValues(
            ChargeLinksCommandBuilder builder,
            ChargeLinksCommandRejectedOutboundMapper sut)
        {
            var chargeCommandRejectedEvent = CreateChargeLinksCommandRejectedEvent(builder);
            var result = (Charges.Infrastructure.Internal.ChargeLinksCommandRejected.ChargeLinksCommandRejected)sut.Convert(chargeCommandRejectedEvent);
            ProtobufAssert.OutgoingContractIsSubset(chargeCommandRejectedEvent, result);
        }

        [Theory]
        [InlineAutoMoqData]
        public void Convert_WhenCalledWithNull_ShouldThrow(ChargeLinksCommandRejectedOutboundMapper sut)
        {
            Assert.Throws<InvalidOperationException>(() => sut.Convert(null!));
        }

        private static ChargeLinksRejectedEvent CreateChargeLinksCommandRejectedEvent(ChargeLinksCommandBuilder builder)
        {
            var chargeLinksCommand = builder.Build();
            var validationErrors = new List<ValidationError>
            {
                new(ValidationRuleIdentifier.ChargeDoesNotExist, listElementWithValidationError: "1"),
                new(ValidationRuleIdentifier.MeteringPointDoesNotExist, null),
            };

            var chargeLinksCommandRejectedEvent = new ChargeLinksRejectedEvent(
                SystemClock.Instance.GetCurrentInstant(),
                chargeLinksCommand,
                validationErrors);
            UpdateInstantsToValidTimes(chargeLinksCommandRejectedEvent);
            return chargeLinksCommandRejectedEvent;
        }

        private static void UpdateInstantsToValidTimes(ChargeLinksRejectedEvent chargeLinksCommandRejectedEvent)
        {
            chargeLinksCommandRejectedEvent.ChargeLinksCommand.Document.RequestDate =
                Instant.FromUtc(2021, 7, 21, 11, 42, 25);
            chargeLinksCommandRejectedEvent.ChargeLinksCommand.Document.CreatedDateTime =
                Instant.FromUtc(2021, 7, 21, 12, 14, 43);
        }
    }
}
