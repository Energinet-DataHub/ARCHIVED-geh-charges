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

namespace GreenEnergyHub.Charges.TestCore
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
                    MarketParticipants.GridAccessProviderOfMeteringPoint571313180000000005.Gln;
            }
        }

        public static class MarketParticipants
        {
            public static class SystemOperator
            {
                public const string Gln = "5790000432752";
                public static readonly Guid Id = new("AF450C03-1937-4EA1-BB66-17B6E4AA51F5");
                /*public static readonly Guid ActorId = new("DD8953B0-14A8-44DA-81E2-7663A99C4E90");*/
            }

            public static class MeteringPointAdministrator
            {
                public const string Gln = "5790001330552";
            }

            public static class GridAccessProviderOfMeteringPoint571313180000000005
            {
                public const string Gln = "8100000000016";
            }

            public static class Inactive8900000000005
            {
                public const string Gln = "8900000000005";
            }
        }

        public static class GridAreaLink
        {
            public static class Provider8100000000030
            {
                public const string MarketParticipantId = "8100000000030";
                public static readonly Guid Id = new("e009b9b2-edce-4f74-b466-98d0bbb0a94a");
                public static readonly Guid GridAreaId = new("cb655a73-090e-4352-b93d-c66a875ca5a0");
            }

            public static class Provider8500000000013
            {
                public static readonly Guid Id = new("a37aca18-51e1-4978-a52b-28ac165f801b");
                public static readonly Guid GridAreaId = new("c13d70cb-8a0e-480e-bd05-3b28b9e3b104");
            }
        }
    }
}
