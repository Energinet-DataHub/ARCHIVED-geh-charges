using System;
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

                    // Overrides the compare of NodatTime instant and protobuf Timestamp
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
