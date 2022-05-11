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

using Energinet.Charges.Contracts.ChargeLink;
using FluentAssertions;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.WebApi.Model.ChargeLink
{
    [UnitTest]
    public class ChargeLinkV2DtoTests
    {
        /// <summary>
        /// See https://github.com/Energinet-DataHub/geh-charges/issues/933 regarding naming decisions.
        /// </summary>
        [Fact]
        public void Props_HaveCorrectNames()
        {
            nameof(ChargeLinkV2Dto.ChargeInformationId).Should().Be("ChargeId");
            nameof(ChargeLinkV2Dto.ChargeName).Should().Be("ChargeName");
            nameof(ChargeLinkV2Dto.Quantity).Should().Be("Quantity");
            nameof(ChargeLinkV2Dto.ChargeOwnerId).Should().Be("ChargeOwnerId");
            nameof(ChargeLinkV2Dto.ChargeType).Should().Be("ChargeType");
            nameof(ChargeLinkV2Dto.TaxIndicator).Should().Be("TaxIndicator");
            nameof(ChargeLinkV2Dto.TransparentInvoicing).Should().Be("TransparentInvoicing");
            nameof(ChargeLinkV2Dto.StartDate).Should().Be("StartDate");
            nameof(ChargeLinkV2Dto.EndDate).Should().Be("EndDate");
        }
    }
}
