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

using FluentAssertions;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using NodaTime;
using Xunit;

namespace GreenEnergyHub.Charges.TestCore
{
    public class AssertExtensions : Assert
    {
        public static void ContractIsEquivalent(object contract, object objectAssertions)
        {
            objectAssertions.Should().BeEquivalentTo(
                contract,
                options =>
                {
                    // Gives more explicit assertion failure
                    options.WithTracing();

                    // Overrides the compare of NodaTime instant and protobuf Timestamp
                    options.Using<object>(s => ((Instant)s.Subject).ToUnixTimeSeconds()
                            .Should().Be(((Timestamp)s.Expectation).Seconds))
                        .WhenTypeIs<Timestamp>();

                    // Enforce member comparision of protobuf objects that override object.Equals
                    options.ComparingByMembers<IMessage>();
                    return options;
                });
        }
    }
}
