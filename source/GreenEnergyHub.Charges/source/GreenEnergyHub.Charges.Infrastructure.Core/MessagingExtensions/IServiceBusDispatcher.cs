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

using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions
{
    // ReSharper disable once UnusedTypeParameter
    // - Type parameter is necessary in order to distinguish instances during resolution of types in dependency container
    public interface IServiceBusDispatcher
    {
        public Task DispatchAsync(ServiceBusMessage serviceBusMessage, CancellationToken cancellationToken = default);
    }
}
