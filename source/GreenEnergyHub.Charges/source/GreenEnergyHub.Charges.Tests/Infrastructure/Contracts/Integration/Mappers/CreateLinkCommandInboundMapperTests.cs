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

using System.Diagnostics.CodeAnalysis;
using Energinet.Charges.Contracts;
using GreenEnergyHub.Charges.Domain.Dtos.CreateLinksRequests;
using GreenEnergyHub.Charges.Infrastructure.Integration.Mappers;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Protobuf;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Contracts.Integration.Mappers
{
    [UnitTest]
    public class CreateLinkCommandInboundMapperTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void Convert_WhenCalled_ShouldMapToDomainObjectWithCorrectValues(
            [NotNull] CreateDefaultChargeLinks createDefaultChargeLinks,
            [NotNull] CreateDefaultChargeLinksInboundMapper sut)
        {
            var result = (CreateLinksRequest)sut.Convert(createDefaultChargeLinks);
            ProtobufAssert.IncomingContractIsSuperset(result, createDefaultChargeLinks);
        }
    }
}
