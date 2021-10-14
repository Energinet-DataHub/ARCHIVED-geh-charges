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
using GreenEnergyHub.Charges.Application.SeedWork.SyncRequest;

namespace GreenEnergyHub.Charges.Infrastructure.SyncRequest
{
    public class SyncRequestMetadata : ISyncRequestMetadata
    {
#pragma warning disable 8618
        public string SessionId { get; set; }
#pragma warning restore 8618

        public void ValidateOrThrow()
        {
            if (SessionId == null)
                throw new InvalidOperationException($"Property '{nameof(SessionId)}' is missing");
        }
    }
}
