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
using System.Security.Cryptography.X509Certificates;

namespace GreenEnergyHub.Charges.IntegrationTests
{
    /// <summary>
    /// Provide a unified compile time safe way to access values corresponding to the seeded data
    /// in the test database.
    /// </summary>
    public static class SeededData
    {
        public static class MeteringPoints
        {
            public static class Mp571313180000000005
            {
                public const string Id = "571313180000000005";

                public const string GridAccessProvider =
                    MarketParticipant.GridAccessProviderOfMeteringPoint571313180000000005;
            }
        }

        public static class MarketParticipant
        {
            public const string GridAccessProviderOfMeteringPoint571313180000000005 = "8100000000016";
            public const string Inactive8900000000005 = "8900000000005";
        }

        public static class GridArea
        {
            public static class Provider8100000000030
            {
                public const string MarketParticipantId = "8100000000030";
                public static readonly Guid Id = new("ed6c94f3-24a8-43b3-913d-bf7513390a32");
            }

            public static class Provider8500000000013
            {
                public static readonly Guid Id = new("c13d70cb-8a0e-480e-bd05-3b28b9e3b104");
            }
        }

        public static class GridAreaLink
        {
            public static class Provider8500000000013
            {
                public static readonly Guid Id = new("a37aca18-51e1-4978-a52b-28ac165f801b");
            }
        }
    }
}
