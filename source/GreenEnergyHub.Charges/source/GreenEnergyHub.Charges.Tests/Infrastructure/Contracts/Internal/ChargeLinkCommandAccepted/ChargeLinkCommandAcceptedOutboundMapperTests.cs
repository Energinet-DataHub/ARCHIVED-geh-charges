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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksAcceptedEvents;
using GreenEnergyHub.Charges.Infrastructure.Contracts.Internal.ChargeLinkCommandAccepted;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Protobuf;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Contracts.Internal.ChargeLinkCommandAccepted
{
    [UnitTest]
    public class ChargeLinkCommandAcceptedOutboundMapperTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void Convert_WhenCalled_ShouldMapToProtobufWithCorrectValues(
            ChargeLinksAcceptedEvent chargeLinksAcceptedEvent,
            ChargeLinksCommandAcceptedOutboundMapper sut)
        {
            var result = (Charges.Infrastructure.Internal.ChargeLinksCommandAccepted.ChargeLinksCommandAccepted)sut
                    .Convert(chargeLinksAcceptedEvent);
            ProtobufAssert.OutgoingContractIsSubset(chargeLinksAcceptedEvent, result);
        }

        [Theory]
        [InlineAutoMoqData]
        public void Convert_WhenCalledWithNull_ShouldThrow(ChargeLinksCommandAcceptedOutboundMapper sut)
        {
            Assert.Throws<InvalidOperationException>(() => sut.Convert(null!));
        }
    }
}
