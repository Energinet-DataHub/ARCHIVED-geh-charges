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
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Acknowledgements;
using GreenEnergyHub.Charges.Infrastructure.Integration.ChargeRejection;
using GreenEnergyHub.Charges.Infrastructure.Integration.Mappers;
using GreenEnergyHub.Charges.TestCore;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Integration.Mappers
{
    [UnitTest]
    public class ChargeRejectionLinkCreatedOutboundMapperTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void Convert_WhenCalled_MapsToCorrectValues(
            [NotNull] ChargeRejection chargeRejection,
            [NotNull] ChargeRejectionOutboundMapper sut)
        {
            // Act
            var result = (ChargeRejectionContract)sut.Convert(chargeRejection);

            // Assert
            result.CorrelationId.Should().BeEquivalentTo(chargeRejection.CorrelationId);
            result.ReceiverMrid.Should().BeEquivalentTo(chargeRejection.ReceiverMRid);
            result.BusinessReasonCode.Should().BeEquivalentTo(chargeRejection.BusinessReasonCode);
            result.OriginalTransactionReferenceMrid.Should().BeEquivalentTo(chargeRejection.OriginalTransactionReferenceMRid);
            result.ReceiverMarketParticipantRole.Should().BeEquivalentTo(chargeRejection.ReceiverMarketParticipantRole);
            result.RejectReasons.Should().BeEquivalentTo(chargeRejection.RejectReason);
        }

        [Fact]
        public void Convert_WhenCalledWithNull_ShouldThrow()
        {
            var mapper = new ChargeRejectionOutboundMapper();
            ChargeRejection? chargeRejection = null;
            Assert.Throws<InvalidOperationException>(() => mapper.Convert(chargeRejection!));
        }
    }
}
