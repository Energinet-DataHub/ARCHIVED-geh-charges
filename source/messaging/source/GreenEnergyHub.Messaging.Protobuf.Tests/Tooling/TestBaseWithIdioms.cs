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

using System.Diagnostics;
using GreenEnergyHub.Messaging.Protobuf.Tests.Tooling.AutoFixture.Extensions;
using Xunit;

namespace GreenEnergyHub.Messaging.Protobuf.Tests.Tooling
{
    /// <summary>
    /// Base class for test classes, which provides a number of features:
    /// * Fixture, an instance of IFixture which provides access to AutoFixture configured with AutoMoq.
    /// * Use the inherited "Sut" property to get a system-under-test instance.
    /// * It tests idioms such as guards for null arguments out of the box.
    /// </summary>
    [DebuggerStepThrough]
    public abstract class TestBaseWithIdioms<TSut> : TestBase<TSut>
        where TSut : class
    {
        protected TestBaseWithIdioms()
        {
            MethodVerificationLevel = MethodVerificationLevel.Disabled;

            WritablePropertyVerificationEnabled = true;
        }

        /// <summary>
        /// Can be used to control which methods (if any) are verified as part of the
        /// guard clause verification.
        /// </summary>
        protected MethodVerificationLevel MethodVerificationLevel { get; set; }

        /// <summary>
        /// Can be used to control whether writable property verification is performed.
        /// </summary>
        protected bool WritablePropertyVerificationEnabled { get; set; }

        /// <summary>
        /// Verify that each parameter is guarded (throws exception if value is invalid).
        /// Can verify constructors and methods (if enabled).
        /// </summary>
        [Fact]
        public void GuardClauseVerification()
        {
            Fixture.GuardClauseVerification<TSut>(MethodVerificationLevel);
        }

        /// <summary>
        /// Verify that each writable property has a get/set, and that the get
        /// returns the object assigned using set.
        /// </summary>
        [Fact]
        public void WritablePropertyVerification()
        {
            if (WritablePropertyVerificationEnabled)
            {
                Fixture.WritablePropertyVerification<TSut>();
            }
        }
    }
}
