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
using System.Collections.Generic;

// ReSharper disable All
namespace GreenEnergyHub.PostOffice.Communicator.Model
{
    /// <summary>
    /// Represents a request for a bundle containing the specified ids.
    /// <param name="IdempotencyId">An unique identifier for this request.</param>
    /// <param name="DataAvailableNotificationIds">The ids that must be contained within the created bundle.</param>
    /// </summary>
    public sealed record DataBundleRequestDto(string IdempotencyId, IEnumerable<Guid> DataAvailableNotificationIds);
}
