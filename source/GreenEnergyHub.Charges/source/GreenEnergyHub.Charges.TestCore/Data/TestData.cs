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

namespace GreenEnergyHub.Charges.TestCore.Data
{
    /// <summary>
    /// Provide a unified compile time safe way to access values corresponding to the seeded test data
    /// in the test database.
    /// </summary>
    public static class TestData
    {
        public static class Charge
        {
            public static class TestTar001
            {
                public const string SenderProvidedChargeId = "TestTar001";
                public const string Name = "Tariff with multiple periods";
                public const int NoOfPeriods = 4;
                public const string ChargeOwnerId = "8100000000030";
            }
        }
    }
}
