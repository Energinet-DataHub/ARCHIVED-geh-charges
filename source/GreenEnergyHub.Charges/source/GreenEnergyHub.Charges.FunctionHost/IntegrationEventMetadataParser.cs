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
using Energinet.DataHub.Core.JsonSerialization;
using Microsoft.Azure.Functions.Worker;
using NodaTime;

namespace GreenEnergyHub.Charges.FunctionHost
{
    internal class IntegrationEventMetadataParser
    {
        private readonly IJsonSerializer _jsonSerializer;

        public IntegrationEventMetadataParser(IJsonSerializer jsonSerializer)
        {
            _jsonSerializer = jsonSerializer;
        }

        public bool TryParse(
            FunctionContext functionContext,
            [NotNullWhen(true)] out IntegrationEventJsonMetadata? userProperties)
        {
            userProperties = null;

            var bindingData = functionContext.BindingContext.BindingData;
            if (bindingData.TryGetValue("UserProperties", out var userPropertiesObject))
            {
                if (userPropertiesObject is string userProps)
                {
                    userProperties = _jsonSerializer.Deserialize<IntegrationEventJsonMetadata>(userProps);
                    return true;
                }
            }

            return false;
        }

        internal sealed class IntegrationEventJsonMetadata
        {
            public IntegrationEventJsonMetadata(
                string messageType,
                Instant operationTimestamp,
                string operationCorrelationId)
            {
                OperationCorrelationId = operationCorrelationId;
                MessageType = messageType;
                OperationTimestamp = operationTimestamp;
            }

            public string MessageType { get; }

            public Instant OperationTimestamp { get; }

            public string OperationCorrelationId { get; }
        }
    }
}
