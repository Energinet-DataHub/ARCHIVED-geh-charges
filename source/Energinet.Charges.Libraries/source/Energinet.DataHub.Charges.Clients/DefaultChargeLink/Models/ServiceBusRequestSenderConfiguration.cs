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
using System.Diagnostics.CodeAnalysis;

namespace Energinet.DataHub.Charges.Clients.DefaultChargeLink.Models
{
    /// <summary>
    /// Configuration needed to address where the Charges domain
    /// is expected to reply, once done with creating default charge links.
    /// </summary>
    public sealed class ServiceBusRequestSenderConfiguration : IServiceBusRequestSenderConfiguration
    {
        public string ReplyQueueName { get; }

        // This is the queue created in the shared environment to receive the request. Only rename in accordance with infrastructure.
        public string RequestQueueName => "create-link-request";

        public ServiceBusRequestSenderConfiguration([DisallowNull] string replyQueueName)
        {
            ReplyQueueName = replyQueueName ?? throw new ArgumentNullException(nameof(replyQueueName));
        }
    }
}
