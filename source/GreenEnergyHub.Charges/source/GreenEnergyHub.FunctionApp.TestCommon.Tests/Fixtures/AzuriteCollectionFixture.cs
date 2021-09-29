﻿// Copyright 2020 Energinet DataHub A/S
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

using Xunit;

namespace GreenEnergyHub.FunctionApp.TestCommon.Tests.Fixtures
{
    /// <summary>
    /// Only one Azurite process can be running at the same time.
    ///
    /// xUnit documentation of collection fixtures:
    ///  * https://xunit.github.io/docs/shared-context
    /// </summary>
    [CollectionDefinition(nameof(AzuriteCollectionFixture))]
    public class AzuriteCollectionFixture
    {
        // This class has no code, and is never created. Its purpose is to carry the information
        // given by [CollectionDefinition] and any ICollectionFixture<> interfaces so
        // we can tag tests classes with [Collection] and thereby link to these information.
    }
}
