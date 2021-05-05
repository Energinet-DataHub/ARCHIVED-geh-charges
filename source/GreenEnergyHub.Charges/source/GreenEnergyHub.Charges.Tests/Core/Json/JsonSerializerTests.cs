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
