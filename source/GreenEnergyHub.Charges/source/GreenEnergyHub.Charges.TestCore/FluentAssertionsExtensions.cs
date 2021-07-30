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

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using FluentAssertions.Primitives;
using Google.Protobuf;
using NodaTime;

namespace GreenEnergyHub.Charges.TestCore
{
    public static class FluentAssertionsExtensions
    {
        public static void BeEquivalentToOutgoing<TExpectation>(
            [NotNull] this ObjectAssertions objectAssertions,
            TExpectation expectation,
            string because = "",
            params object[] becauseArgs)
        {
            objectAssertions.BeEquivalentTo(
                expectation,
                options =>
                {
                    // Gives more explicit assertion failure
                    options.WithTracing();

                    // Overrides the compare of NodaTime instant and protobuf Timestamp
                    options.Using<object>(s => ((Instant)s.Subject).ToUnixTimeSeconds()
                        .Should().Be(((Google.Protobuf.WellKnownTypes.Timestamp)s.Expectation).Seconds))
                        .WhenTypeIs<Google.Protobuf.WellKnownTypes.Timestamp>();

                    // Enforce member comparision of protobuf objects that override object.Equals
                    options.ComparingByMembers<IMessage>();
                    return options;
                },
                because,
                becauseArgs);
        }
    }
}
