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

using GreenEnergyHub.Charges.Domain.Events.Local;
using GreenEnergyHub.TestHelpers.Traits;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Xunit;
using JsonSerializer = GreenEnergyHub.Charges.Core.Json.JsonSerializer;

namespace GreenEnergyHub.Charges.Tests.Core.Json
{
    [Trait(TraitNames.Category, TraitValues.UnitTest)]
    public class JsonSerializerTests
    {
        private const string ChargeCommandAcceptedEventMRid = "MAR2021-05-05T13:56:29.301Z";

        private const string ChargeCommandAcceptedEventJsonString = @"
{
  ""Command"": {
        ""MarketDocument"": {
            ""mRID"": ""MD2021-05-05T13:56:29.301Z"",
            ""CreatedDateTime"": ""2021-05-05T13:56:29.301Z"",
            ""Sender_MarketParticipant"": {
                ""Id"": 37,
                ""MRid"": ""8100000000030"",
                ""Name"": ""Grid Operator 3"",
                ""Role"": 0
            },
            ""Receiver_MarketParticipant"": {
                ""Id"": 1,
                ""MRid"": ""5790001330552"",
                ""Name"": ""Hub"",
                ""Role"": 0
            },
            ""ProcessType"": 18,
            ""Market_ServiceCategoryKind"": 23
        },
    ""MktActivityRecord"": {
    ""MRid"": ""MAR2021-05-05T13:56:29.301Z"",
    ""ValidityStartDate"": ""2021-07-31T22:00:00Z"",
    ""ValidityEndDate"": null,
    ""Status"": 2,
    ""ChargeType"": {
    ""name"": ""myEbixDescription"",
    ""description"": ""myEbixLongDescription"",
    ""VATPayer"": ""D02"",
    ""TransparentInvoicing"": true,
    ""TaxIndicator"": false
}
},
""Type"": ""D02"",
""ChargeType_mRID"": ""VoltFee052"",
""ChargeTypeOwner_mRID"": ""8100000000030"",
""Period"": {
    ""Resolution"": ""P1D"",
    ""Points"": [
    {
        ""Position"": 1,
        ""PriceAmount"": 200.003,
        ""Time"": ""2021-07-31T22:00:00Z""
    }
    ]
},
""RequestDate"": ""2021-05-05T13:56:29.301Z"",
""LastUpdatedBy"": ""LastUpdatedBy"",
""Transaction"": {
    ""MRID"": ""T2021-05-05T13:56:29.301Z""
},
""CorrelationId"": ""4b8cfcd1-59f7-4931-8f77-99879b5ff3d8""
},
""PublishedTime"": ""2021-05-05T13:56:29.5768661Z"",
""Transaction"": {
    ""MRID"": ""5b66f3db3cd84bda9858e1b8886458e9""
},
""CorrelationId"": ""4b8cfcd1-59f7-4931-8f77-99879b5ff3d8""
}
";

        [Fact]
        public void Deserialize_WhenSetterIsPublic_SetsProp()
        {
            var actual = new JsonSerializer().Deserialize<Foo>("{\"PublicSetter\": 2}");
            Assert.Equal(2, actual.PublicSetter);
        }

        [Fact]
        public void Deserialize_WhenSetterIsPrivate_SetsProp()
        {
            var actual = new JsonSerializer().Deserialize<Foo>("{\"PrivateSetter\": 3}");
            Assert.Equal(3, actual.PrivateSetter);
        }

        [Fact]
        public void Deserialize_WhenNoSetter_SetsPropUsingCtor()
        {
            var actual = new JsonSerializer().Deserialize<Foo>("{\"NoSetter\": 4}");
            Assert.Equal(4, actual.NoSetter);
        }

        [Fact]
        public void Deserialize_DeserializesChargeCommandAcceptedEvent()
        {
            var actual =
                new JsonSerializer().Deserialize<ChargeCommandAcceptedEvent>(ChargeCommandAcceptedEventJsonString);
            Assert.Equal(ChargeCommandAcceptedEventMRid, actual.Command.MktActivityRecord.MRid);
        }

        [UsedImplicitly]
        private class Foo
        {
            public Foo(int noSetter)
            {
                NoSetter = noSetter;
            }

            public int PublicSetter { get; set; }

            [JsonProperty]
            public int PrivateSetter
            {
                get;
                [UsedImplicitly]
                private set;
            }

            public int NoSetter { get; }
        }
    }
}
