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

using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandAcceptedEvents;
using Newtonsoft.Json;
using Xunit;
using Xunit.Categories;
using JsonSerializer = GreenEnergyHub.Charges.Core.Json.JsonSerializer;

namespace GreenEnergyHub.Charges.Tests.Core.Json
{
    [UnitTest]
    public class JsonSerializerTests
    {
        private const string ChargeCommandAcceptedEventMRid = "MAR2021-05-05T13:56:29.301Z";

        private const string ChargeCommandAcceptedEventJsonString = @"
{
  ""Command"": {
        ""Document"": {
		    ""Id"": ""MAR2021-05-05T13:56:29.301Z"",
		    ""CreatedDateTime"": ""2021-05-05T13:56:29.301Z"",
            ""BusinessReasonCode"": 1,
		    ""IndustryClassification"": 23,
            ""Type"": 10,
		    ""Sender"": {
		    	""Id"": ""8100000000030"",
		    	""Name"": ""Grid Operator 3"",
		    	""BusinessProcessRole"": 0
		    },
		    ""Recipient"": {
		    	""Id"": ""5790001330552"",
		    	""Name"": ""Hub"",
		    	""BusinessProcessRole"": 0
		    }
	},
	""ChargeOperation"": {
		    ""Id"": ""MD2021-05-05T13:56:29.301Z"",
		    ""StartDateTime"": ""2021-06-30T22:00:00Z"",
		    ""EndDateTime"": ""9999-12-31T23:59:59Z"",
		    ""OperationType"": 2,
            ""ChargeId"": ""VoltTPostman978"",
            ""ChargeName"": ""Electric charge"",
            ""ChargeType"": 2,
  	        ""ChargeOwner"": ""8100000000030"",
            ""TaxIndicator"": true,
            ""VatClassification"": 0,
            ""Description"": ""The charge description"",
            ""Period"": {
		        ""Resolution"": ""P1D"",
		        ""Points"": [
		    	    {
		    		    ""Position"": 1,
		    		    ""PriceAmount"": 150.001,
		    		    ""Time"": ""2021-04-30T22:00:00Z""
		    	    }
		        ]
	        }
    },
   ""Transaction"": {
   		""mRID"": ""MD2021-05-05T13:56:29.301Z""
   	}
}
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
            Assert.Equal(ChargeCommandAcceptedEventMRid, actual.Command.Document.Id);
        }

        private sealed class Foo
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
                private set;
            }

            public int NoSetter { get; }
        }
    }
}
