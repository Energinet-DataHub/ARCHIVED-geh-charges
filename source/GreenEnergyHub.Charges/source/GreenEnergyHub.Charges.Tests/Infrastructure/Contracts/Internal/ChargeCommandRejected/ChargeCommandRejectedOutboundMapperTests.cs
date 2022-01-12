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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandRejectedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation;
using GreenEnergyHub.Charges.Infrastructure.Contracts.Internal.ChargeCommandRejected;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeCommandRejected;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Builders;
using GreenEnergyHub.Charges.Tests.Protobuf;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Contracts.Internal.ChargeCommandRejected
{
    [UnitTest]
    public class ChargeCommandRejectedOutboundMapperTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void Convert_WhenCalled_ShouldMapToProtobufWithCorrectValues(
            ChargeCommandBuilder builder,
            ChargeCommandRejectedOutboundMapper sut)
        {
            var chargeCommandRejectedEvent = CreateChargeCommandRejectedEvent(builder);
            var result = (ChargeCommandRejectedContract)sut.Convert(chargeCommandRejectedEvent);
            ProtobufAssert.OutgoingContractIsSubset(chargeCommandRejectedEvent, result);
        }

        [Theory]
        [InlineAutoMoqData]
        public void Convert_WhenCalledWithNull_ShouldThrow(ChargeCommandRejectedOutboundMapper sut)
        {
            Assert.Throws<InvalidOperationException>(() => sut.Convert(null!));
        }

        private static ChargeCommandRejectedEvent CreateChargeCommandRejectedEvent(ChargeCommandBuilder builder)
        {
            var chargeCommand = builder.Build();
            var validationRuleIdentifiers = new List<ValidationError>
            {
                new(ValidationRuleIdentifier.MaximumPrice, listElementWithValidationError: "2"),
                new(ValidationRuleIdentifier.ResolutionFeeValidation, null),
            };
            var chargeCommandRejectedEvent = new ChargeCommandRejectedEvent(
                SystemClock.Instance.GetCurrentInstant(),
                chargeCommand,
                validationRuleIdentifiers);
            UpdateInstantsToValidTimes(chargeCommandRejectedEvent);
            return chargeCommandRejectedEvent;
        }

        private static void UpdateInstantsToValidTimes(ChargeCommandRejectedEvent chargeCommandRejectedEvent)
        {
            chargeCommandRejectedEvent.Command.Document.RequestDate = Instant.FromUtc(2021, 7, 21, 11, 42, 25);
            chargeCommandRejectedEvent.Command.Document.CreatedDateTime = Instant.FromUtc(2021, 7, 21, 12, 14, 43);
        }
    }
}
