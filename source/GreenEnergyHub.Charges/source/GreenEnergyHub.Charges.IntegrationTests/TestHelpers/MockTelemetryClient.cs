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
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ResourceProvider;
using Google.Protobuf;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace GreenEnergyHub.Charges.IntegrationTests.TestHelpers
{
    public static class MockTelemetryClient
    {
        public static async Task SendWrappedServiceBusMessageToQueueAsync(QueueResource queue, ServiceBusMessage serviceBusMessage, string correlationId, string parentId)
        {
            var telemetryClient = Create();
            var operation = telemetryClient.StartOperation<DependencyTelemetry>("MyTest", correlationId, parentId);
            operation.Telemetry.Type = "Function";
            try
            {
                operation.Telemetry.Success = true;
                await queue.SenderClient.SendMessageAsync(serviceBusMessage, CancellationToken.None);
            }
            catch (Exception exception)
            {
                operation.Telemetry.Success = false;
                telemetryClient.TrackException(exception);
                throw;
            }
            finally
            {
                telemetryClient.StopOperation(operation);
            }
        }

        private static TelemetryClient Create()
        {
            var mockTelemetryChannel = new MockTelemetryChannel();
            var mockTelemetryConfig = new TelemetryConfiguration
            {
                TelemetryChannel = mockTelemetryChannel,
                InstrumentationKey = Guid.NewGuid().ToString(),
            };

            var mockTelemetryClient = new TelemetryClient(mockTelemetryConfig);
            return mockTelemetryClient;
        }
    }
}
