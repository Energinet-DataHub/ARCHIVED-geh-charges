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

namespace GreenEnergyHub.Charges.Application.SeedWork.SyncRequest
{
    /// <summary>
    /// Meta data available for synchronous inter-domain requests.
    /// That is for requests in a request-response communication.
    ///
    /// This is in contrast to the asynchronous request-reply
    /// communication pattern.
    /// </summary>
    public interface ISyncRequestMetadata
    {
        /// <summary>
        /// Session ID is needed by the underlying communication
        /// protocol (Azure Service Bus message sessions, see
        /// https://docs.microsoft.com/en-us/azure/service-bus-messaging/message-sessions).
        /// </summary>
        string SessionId { get; }
    }
}
