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
using System.Reflection;
using System.Runtime.CompilerServices;
using AutoFixture;
using AutoFixture.Idioms;
using GreenEnergyHub.Messaging.Protobuf.Tests.Tooling.AutoFixture.Behaviors;

namespace GreenEnergyHub.Messaging.Protobuf.Tests.Tooling.AutoFixture.Extensions
{
    public static class AutoFixtureIdiomsExtensions
    {
        /// <summary>
        /// Verify that each parameter is guarded (throws exception if value is invalid).
        /// Verify constructors, and can verify methods (if enabled).
        /// </summary>
        /// <typeparam name="TSut">The type who's constructors and methods are verified.</typeparam>
        /// <param name="fixture">The fixture used for the configuration of the guard clause assertion.</param>
        /// <param name="methodVerificationLevel">Determines if methods are verified, and whether it includes methods with the "async" keyword.</param>
        public static void GuardClauseVerification<TSut>(this IFixture fixture, MethodVerificationLevel methodVerificationLevel = MethodVerificationLevel.Disabled)
        {
            var assertion = new GuardClauseAssertion(
                fixture,
                new CompositeBehaviorExpectation(
                    new ArgumentNullAndArgumentExceptionBehaviorExpectation(),
                    new EmptyGuidBehaviorExpectation()));

            // Assert => Constructors
            assertion.Verify(typeof(TSut).GetConstructors());

            if (methodVerificationLevel != MethodVerificationLevel.Disabled)
            {
                var methods = typeof(TSut).GetMethods(
                    BindingFlags.DeclaredOnly |
                    BindingFlags.Public |
                    BindingFlags.Instance)
                    .ToList();

                methods = methods.Where(info =>
                    !info.Name.StartsWith("set_", StringComparison.Ordinal)
                    && !info.Name.StartsWith("add_", StringComparison.Ordinal)
                    && !info.Name.StartsWith("remove_", StringComparison.Ordinal))
                    .ToList();

                if (methodVerificationLevel == MethodVerificationLevel.EnabledForNonAsyncMethods)
                {
                    methods = methods.Where(info =>
                        !info.CustomAttributes.Any(x => x.AttributeType == typeof(AsyncStateMachineAttribute)))
                        .ToList();
                }

                // Assert => Methods
                assertion.Verify(methods);
            }
        }

        /// <summary>
        /// Verify that each writable property has a get/set, and that the get
        /// returns the object assigned using set.
        /// </summary>
        /// <typeparam name="TSut">The type who's properties are verified.</typeparam>
        /// <param name="fixture">The fixture used for the configuration of the writable property assertion.</param>
        public static void WritablePropertyVerification<TSut>(this IFixture fixture)
        {
            var assertion = new WritablePropertyAssertion(fixture);

            // Assert => Writable properties
            assertion.Verify(typeof(TSut).GetProperties());
        }
    }
}
