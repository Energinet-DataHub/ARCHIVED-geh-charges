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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksAcceptedEvents;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandAccepted;
using GreenEnergyHub.Charges.Infrastructure.Internal.Mappers;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Protobuf;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Internal.Mappers
{
    [UnitTest]
    public class LinkCommandAcceptedInboundMapperTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void Convert_WhenCalled_ShouldMapToDomainObjectWithCorrectValues(
            [NotNull] ChargeLinkCommandAccepted chargeLinkCommandAccepted,
            [NotNull] LinkCommandAcceptedInboundMapper sut)
        {
            FixGuidStrings(chargeLinkCommandAccepted);

            var result = (ChargeLinksAcceptedEvent)sut.Convert(chargeLinkCommandAccepted);
            ProtobufAssert.IncomingContractIsSuperset(result, chargeLinkCommandAccepted);
        }

        [Theory]
        [InlineAutoMoqData]
        public void Convert_WhenCalledWithNull_ShouldThrow([NotNull]LinkCommandAcceptedInboundMapper sut)
        {
            Assert.Throws<InvalidOperationException>(() => sut.Convert(null!));
        }

        /// <summary>
        /// We need to represent Guid's as strings in contract. This leads to the need to fix the auto created Guid strings.
        /// Also see https://docs.microsoft.com/en-us/dotnet/architecture/grpc-for-wcf-developers/protobuf-data-types#systemguid
        /// </summary>
        private static void FixGuidStrings(ChargeLinkCommandAccepted chargeLinkCommandAccepted)
        {
            foreach (var chargeLink in chargeLinkCommandAccepted.ChargeLinksCommand.ChargeLinks)
            {
                chargeLink.ChargeId = Guid.NewGuid().ToString();
            }
        }
    }
}
