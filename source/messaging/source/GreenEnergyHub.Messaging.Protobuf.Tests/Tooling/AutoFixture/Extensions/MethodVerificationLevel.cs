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

namespace GreenEnergyHub.Messaging.Protobuf.Tests.Tooling.AutoFixture.Extensions
{
    /// <summary>
    /// Used for controlling the method verification level when performing a GuardClauseVerification.
    /// </summary>
    public enum MethodVerificationLevel
    {
        /// <summary>
        /// Method verifiction disabled.
        /// </summary>
        Disabled = 0,

        /// <summary>
        /// Method verification enabled for methods that does not include the "async" keyword.
        /// See also: https://github.com/AutoFixture/AutoFixture/issues/268
        /// </summary>
        EnabledForNonAsyncMethods,

        /// <summary>
        /// Method verification enabled for all methods, meaning including any with the "async" keyword.
        /// See also: https://github.com/AutoFixture/AutoFixture/issues/268
        /// </summary>
        EnabledForAllMethods,
    }
}
