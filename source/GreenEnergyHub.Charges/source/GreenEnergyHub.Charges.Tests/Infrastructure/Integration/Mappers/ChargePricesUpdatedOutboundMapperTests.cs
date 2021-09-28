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
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Infrastructure.Integration.Mappers;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.TestCore.Protobuf;
using NodaTime;
using Xunit;
using Xunit.Categories;
using ChargeType = GreenEnergyHub.Charges.Domain.Charges.ChargeType;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Integration.Mappers
{
    [UnitTest]
    public class ChargePricesUpdatedOutboundMapperTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void Convert_WhenCalled_MapsToCorrectValues(
            [NotNull] ChargePricesUpdatedOutboundMapper sut)
        {
            // Arrange
            var chargePricesUpdated = GetChargePricesUpdated();

            // Act
            var actual = (GreenEnergyHub.Charges.Infrastructure.Integration.ChargeConfirmation.ChargePricesUpdated)sut.Convert(chargePricesUpdated);

            // Assert
            ProtobufAssert.OutgoingContractIsSubset(chargePricesUpdated, actual);
        }

        [Theory]
        [InlineAutoMoqData]
        public void Convert_WhenCalledWithNull_ShouldThrow([NotNull]ChargePricesUpdatedOutboundMapper sut)
        {
            Assert.Throws<InvalidOperationException>(() => sut.Convert(null!));
        }

        private static GreenEnergyHub.Charges.Domain.Charges.Acknowledgements.ChargePricesUpdated GetChargePricesUpdated()
        {
            return new GreenEnergyHub.Charges.Domain.Charges.Acknowledgements.ChargePricesUpdated(
                "chargeId",
                ChargeType.Fee,
                "owner",
                Instant.FromUtc(2021, 8, 15, 22, 0, 0),
                Instant.FromUtc(2021, 9, 17, 22, 0, 0),
                new List<Point>
                {
                    new () { Position = 1, Price = 2.2m, Time = Instant.FromUtc(2021, 8, 15, 22, 0, 0) },
                    new () { Position = 1, Price = 2.2m, Time = Instant.FromUtc(2021, 9, 17, 22, 0, 0) },
                },
                "cor id");
        }
    }
}
