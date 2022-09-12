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

using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.Database;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeLinksReceiptData;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.Repositories
{
    // ReSharper disable once UnusedType.Global
    // This test is implicitly used by calling abstract class AvailableDataRepositoryTests with
    // concrete implementation of type AvailableDataBase
    [IntegrationTest]
    public class AvailableChargeLinkReceiptDataRepositoryTests : AvailableDataRepositoryTests<AvailableChargeLinksReceiptData>
    {
        public AvailableChargeLinkReceiptDataRepositoryTests(MessageHubDatabaseFixture fixture)
            : base(fixture)
        {
        }
    }
}
