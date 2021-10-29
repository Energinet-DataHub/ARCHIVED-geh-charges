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

using System.Collections.Generic;
using GreenEnergyHub.Charges.Application.SeedWork.SyncRequest;
using GreenEnergyHub.Json;
using Microsoft.Azure.Functions.Worker;

namespace GreenEnergyHub.Charges.Infrastructure.SyncRequest
{
    public class SyncRequestMetaDataFactory : ISyncRequestMetaDataFactory
    {
        private readonly IJsonSerializer _jsonSerializer;

        public SyncRequestMetaDataFactory(IJsonSerializer jsonSerializer)
        {
            _jsonSerializer = jsonSerializer;
        }

        public ISyncRequestMetadata Create(FunctionContext functionContext)
        {
            var session = (string)functionContext.BindingContext.BindingData["MessageSession"]!;
            var sessionData = _jsonSerializer.Deserialize<Dictionary<string, object>>(session);
            var sessionId = sessionData["SessionId"].ToString()!;

            return new SyncRequestMetadata() { SessionId = sessionId, };
        }
    }
}
