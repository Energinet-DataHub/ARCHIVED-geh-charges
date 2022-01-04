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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandRejectedEvents;
using GreenEnergyHub.Charges.Infrastructure.Contracts.Internal.ChargeCommandRejected;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeCommandRejected;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Protobuf;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Contracts.Internal.ChargeCommandRejected
{
    [UnitTest]
    public class ChargeCommandRejectedInboundMapperTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void Convert_WhenCalled_ShouldMapToDomainObjectWithCorrectValues(
            ChargeCommandRejectedContract rejectedContract,
            ChargeCommandRejectedInboundMapper sut)
        {
            var result = (ChargeCommandRejectedEvent)sut.Convert(rejectedContract);
            ProtobufAssert.IncomingContractIsSuperset(result, rejectedContract);
        }

        [Theory]
        [InlineAutoMoqData]
        public void Convert_WhenCalledWithNull_ShouldThrow(ChargeCommandRejectedInboundMapper sut)
        {
            Assert.Throws<InvalidOperationException>(() => sut.Convert(null!));
        }
    }
}
