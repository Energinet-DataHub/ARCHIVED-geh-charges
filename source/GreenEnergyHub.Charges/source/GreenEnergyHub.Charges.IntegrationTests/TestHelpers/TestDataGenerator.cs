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
using System.Linq;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.IntegrationTests.Fixtures.FunctionApp;

namespace GreenEnergyHub.Charges.IntegrationTests.TestHelpers
{
    public static class TestDataGenerator
    {
        public const string TestActorId = "ed6c94f3-24a8-43b3-913d-bf7513390a32";

        public static void GenerateDataForIntegrationTests(ChargesFunctionAppFixture fixture)
        {
            var dbContext = fixture.DatabaseManager.CreateDbContext();

            var testActorId = new Guid(TestActorId);
            var marketParticipant = dbContext.MarketParticipants.FirstOrDefault(x => x.Id == testActorId);
            if (marketParticipant != null) return;

            dbContext.MarketParticipants.Add(new MarketParticipant(
                testActorId,
                "MarketParticipantForIntegrationTest",
                true,
                MarketParticipantRole.GridAccessProvider));
            dbContext.SaveChanges();
        }
    }
}
