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
using GreenEnergyHub.Charges.Infrastructure.Messaging;
using JetBrains.Annotations;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Messaging
{
    public class TestableServiceBusChannel : ServiceBusChannel
    {
        public TestableServiceBusChannel(
            [NotNull] ServiceBusSender serviceBusSender,
            [NotNull] ICorrelationContext correlationContext)
            : base(serviceBusSender, correlationContext)
        {
        }

        public async Task WriteToAsync(byte[] data, CancellationToken cancellationToken = default)
        {
            await WriteAsync(data, cancellationToken).ConfigureAwait(false);
        }
    }
}
