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
using System.Diagnostics.CodeAnalysis;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandReceivedEvents;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeCommandReceived;
using GreenEnergyHub.Charges.Infrastructure.Internal.Mappers;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Protobuf;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Contracts.Internal.Mappers
{
    [UnitTest]
    public class ChargeCommandReceivedInboundMapperTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void Convert_WhenCalled_ShouldMapToDomainObjectWithCorrectValues(
            [NotNull] ChargeCommandReceivedContract chargeCommandReceivedContract,
            [NotNull] ChargeCommandReceivedInboundMapper sut)
        {
            var result = (ChargeCommandReceivedEvent)sut.Convert(chargeCommandReceivedContract);
            ProtobufAssert.IncomingContractIsSuperset(result, chargeCommandReceivedContract);
        }

        [Theory]
        [InlineAutoMoqData]
        public void Convert_WhenCalledWithNull_ShouldThrow([NotNull]ChargeCommandReceivedInboundMapper sut)
        {
            Assert.Throws<InvalidOperationException>(() => sut.Convert(null!));
        }
    }
}
