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

using GreenEnergyHub.Charges.FunctionHost.Charges;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.FunctionHost
{
    [IntegrationTest]
    public class FunctionsShouldBeIncludedInCoverageTests
    {
        /// <summary>
        /// The main purpose of this test is to drive the codecoverage to show less misleading numbers
        /// for this repo.
        ///
        /// The test more or less randomly just selected some symbols from each function in order
        /// to pull the projects in and thus have them count in the coverage metrics.
        ///
        /// The test method can eventually be removed when other tests are added for all projects.
        /// </summary>
        [Fact]
        public void AllFunctionsShouldBeIncludedInCoverage()
        {
            Assert.Equal("ChargeInformationCommandReceiverEndpoint", ChargeInformationCommandReceiverEndpoint.FunctionName);
        }
    }
}
