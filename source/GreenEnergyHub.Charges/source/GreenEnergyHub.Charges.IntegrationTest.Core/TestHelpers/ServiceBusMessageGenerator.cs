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
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.JsonSerialization;
using GreenEnergyHub.Charges.Domain.Dtos.Messages.Events;

namespace GreenEnergyHub.Charges.IntegrationTest.Core.TestHelpers
{
    public static class ServiceBusMessageGenerator
    {
        public static ServiceBusMessage CreateWithJsonContent(
            IInternalEvent internalEvent,
            Dictionary<string, string> applicationProperties,
            string correlationId,
            string subject)
        {
            var jsonSerializer = new JsonSerializer();
            var body = jsonSerializer.Serialize(internalEvent);

            var serviceBusMessage = new ServiceBusMessage(body)
            {
                Subject = subject,
                CorrelationId = correlationId,
            };
            foreach (var applicationProperty in applicationProperties)
            {
                serviceBusMessage.ApplicationProperties.Add(applicationProperty.Key, applicationProperty.Value);
            }

            return serviceBusMessage;
        }
    }
}
