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

using Energinet.DataHub.Core.Messaging.Transport;
using GreenEnergyHub.Charges.Application;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandAcceptedEvents;
using GreenEnergyHub.Charges.Infrastructure.Correlation;
using GreenEnergyHub.Charges.Infrastructure.MessageMetaData;
using GreenEnergyHub.Charges.Infrastructure.Messaging.Serialization;
using GreenEnergyHub.Json;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;

namespace GreenEnergyHub.Charges.Infrastructure.Messaging.Registration
{
    public static class RegistrationExtensions
    {
        public static MessagingRegistrator AddMessaging(this Container container)
        {
            container.Register<ICorrelationContext, CorrelationContext>(Lifestyle.Scoped);
            container.Register<IMessageMetaDataContext, MessageMetaDataContext>(Lifestyle.Scoped);
            container.Register<MessageExtractor>(Lifestyle.Scoped);
            container.Register<IJsonSerializer, Core.Json.JsonSerializer>(Lifestyle.Singleton);
            container.Register<MessageDeserializer, JsonMessageDeserializer<ChargeCommandAcceptedEvent>>(Lifestyle.Scoped);
            return new MessagingRegistrator(container);
        }

        public static MessagingRegistrator AddMessagingProtobuf(this Container container)
        {
            container.Register<ICorrelationContext, CorrelationContext>(Lifestyle.Scoped);
            container.Register<IMessageMetaDataContext, MessageMetaDataContext>(Lifestyle.Scoped);
            container.Register<MessageExtractor>(Lifestyle.Scoped);

            return new MessagingRegistrator(container);
        }
    }
}
