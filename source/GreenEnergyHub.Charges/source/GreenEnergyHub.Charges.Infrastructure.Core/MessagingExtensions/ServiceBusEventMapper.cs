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
using System.Linq;

namespace GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions
{
    public class ServiceBusEventMapper
    {
        private readonly HashSet<EventMetadata> _eventMap = new();

        public void Add(Type eventType, string serviceBusTopicName)
        {
            _eventMap.Add(new EventMetadata(eventType, serviceBusTopicName));
        }

        public EventMetadata GetByType(Type eventType)
        {
            if (eventType == null) throw new ArgumentNullException(nameof(eventType));
            return _eventMap
                       .FirstOrDefault(metadata => metadata.EventType == eventType) ??
                   throw new InvalidOperationException(
                       $"No event metadata is registered for type {eventType.FullName}");
        }
    }

    public record EventMetadata(Type EventType, string TopicName);
}
