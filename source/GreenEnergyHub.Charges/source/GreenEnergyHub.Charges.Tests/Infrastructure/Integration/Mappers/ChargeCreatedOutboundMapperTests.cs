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
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Charges.Acknowledgements;
using GreenEnergyHub.Charges.Infrastructure.Integration.ChargeCreated;
using GreenEnergyHub.Charges.Infrastructure.Integration.Mappers;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.TestCore.Protobuf;
using NodaTime;
using Xunit;
using Xunit.Categories;
using Period = GreenEnergyHub.Charges.Domain.Charges.Period;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Integration.Mappers
{
    [UnitTest]
    public class ChargeCreatedOutboundMapperTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void Convert_WhenCalled_MapsToCorrectValues(
            [NotNull] ChargeCreatedOutboundMapper sut)
        {
            var createdEvent = GetChargeCreated();
            var result = (ChargeCreatedContract)sut.Convert(createdEvent);
            ProtobufAssert.OutgoingContractIsSubset(createdEvent, result);
        }

        [Theory]
        [InlineAutoMoqData]
        public void Convert_WhenCalledWithNull_ShouldThrow([NotNull]ChargeCreatedOutboundMapper sut)
        {
            Assert.Throws<InvalidOperationException>(() => sut.Convert(null!));
        }

        private static ChargeCreated GetChargeCreated()
        {
            return new ChargeCreated(
                "chargeId",
                ChargeType.Tariff,
                "chargeOwner",
                "dkk",
                Resolution.PT15M,
                true,
                new Period(
                    Instant.FromUtc(2021, 8, 31, 22, 0, 0),
                    Instant.FromUtc(2021, 9, 30, 22, 0, 0)),
                "sdf");
        }
    }
}
