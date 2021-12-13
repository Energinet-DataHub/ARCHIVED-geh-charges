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
using FluentAssertions;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using GreenEnergyHub.Charges.Core.Enumeration;
using GreenEnergyHub.Charges.Infrastructure.Integration.ChargeConfirmation;
using NodaTime;
using Xunit;
using Xunit.Sdk;

namespace GreenEnergyHub.Charges.Tests.Protobuf
{
    public static class ProtobufAssert
    {
        /// <summary>
        /// Throws <see cref="XunitException"/>.
        /// </summary>
        public static void IncomingContractIsSuperset<TContract>(object expected, IMessage<TContract> actualContract)
            where TContract : IMessage<TContract>
        {
            // FluentAssertions will fail if expected has public props that actualContract doesn't have
            actualContract.Should().BeEquivalentTo(
                expected,
                options =>
                {
                    // Gives more explicit assertion failure
                    options.WithTracing();

                    // Overrides the compare of NodaTime instant and protobuf Timestamp
                    options.Using<object>(s => ((Instant)s.Expectation).ToUnixTimeSeconds()
                            .Should().Be(((Timestamp)s.Subject).Seconds))
                        .WhenTypeIs<Instant>();

                    // Overrides the compare of Guid and protobuf string
                    // Also see https://docs.microsoft.com/en-us/dotnet/architecture/grpc-for-wcf-developers/protobuf-data-types#systemguid
                    options.Using<object>(s => ((Guid)s.Expectation).ToString()
                            .Should().Be((string)s.Subject))
                        .WhenTypeIs<Guid>();

                    // Enforce member comparision of protobuf objects that override object.Equals
                    options.ComparingByMembers<IMessage>();

                    // Ignore the public prop "Descriptor" of the contract object
                    options.Excluding(ctx => ctx.Path == "Descriptor");

                    // Ignore transaction properties from the GreenEnergyHub.Messaging assembly
                    options.Excluding(ctx =>
                        ctx.Path.Split(".", StringSplitOptions.RemoveEmptyEntries).Contains("Transaction") &&
                        ctx.Type.Assembly.FullName!.StartsWith("Energinet.DataHub.Core.Messaging", StringComparison.InvariantCulture));

                    // Use runtime type of "expected"
                    options.RespectingRuntimeTypes();

                    return options;
                });
        }

        /// <summary>
        /// Throws <see cref="XunitException"/>.
        /// </summary>
        public static void OutgoingContractIsSubset<TContract>(object expected, IMessage<TContract> actualContract)
            where TContract : IMessage<TContract>
        {
            // FluentAssertions will fail if actualContract has public props that expected doesn't have
            expected.Should().BeEquivalentTo(
                actualContract,
                options =>
                {
                    // Gives more explicit assertion failure
                    options.WithTracing();

                    // Overrides the compare of NodaTime instant and protobuf Timestamp
                    options.Using<object>(s => ((Instant)s.Subject).ToUnixTimeSeconds()
                            .Should().Be(((Timestamp)s.Expectation).Seconds))
                        .WhenTypeIs<Timestamp>();

                    // Overrides the compare of decimal and the custom DecimalValue
                    options.Using<object>(s => ((decimal)s.Subject)
                            .Should().Be(((DecimalValue)s.Expectation).ToDecimal()))
                        .WhenTypeIs<DecimalValue>();

                    // Enforce member comparision of protobuf objects that override object.Equals
                    options.ComparingByMembers<IMessage>();

                    // Ignore the public prop "Descriptor" of the contract object
                    options.Excluding(ctx => ctx.Path == "Descriptor");

                    // Use runtime type of "expected"
                    options.RespectingRuntimeTypes();

                    return options;
                });
        }

        public static void ContractEnumIsSubSet<TProtobufEnum, TComparisonEnum>()
        {
            Assert.True(
                EnumComparison.IsSubsetOf(
                    typeof(TProtobufEnum),
                    typeof(TComparisonEnum),
                    EnumComparisonStrategy.ProtobufLenient),
                "Checking " + typeof(TProtobufEnum) + " against " + typeof(TComparisonEnum));
        }
    }
}
