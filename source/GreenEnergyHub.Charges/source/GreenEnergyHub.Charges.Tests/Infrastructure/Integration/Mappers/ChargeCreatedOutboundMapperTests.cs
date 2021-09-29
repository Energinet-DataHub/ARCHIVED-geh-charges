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
using GreenEnergyHub.Charges.Infrastructure.Integration.Mappers;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.TestCore.Protobuf;
using NodaTime;
using Xunit;
using Xunit.Categories;
using ChargeType = GreenEnergyHub.Charges.Domain.Charges.ChargeType;
using Period = GreenEnergyHub.Charges.Core.Period;
using Resolution = GreenEnergyHub.Charges.Domain.Charges.Resolution;

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
            var result = (GreenEnergyHub.Charges.Infrastructure.Integration.ChargeCreated.ChargeCreated)sut.Convert(createdEvent);
            ProtobufAssert.OutgoingContractIsSubset(createdEvent, result);
        }

        [Theory]
        [InlineAutoMoqData]
        public void Convert_WhenCalledWithNull_ShouldThrow([NotNull]ChargeCreatedOutboundMapper sut)
        {
            Assert.Throws<InvalidOperationException>(() => sut.Convert(null!));
        }

        private static GreenEnergyHub.Charges.Domain.Charges.Acknowledgements.ChargeCreated GetChargeCreated()
        {
            return new GreenEnergyHub.Charges.Domain.Charges.Acknowledgements.ChargeCreated(
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
