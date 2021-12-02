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
using System.Reflection;
using Energinet.DataHub.Core.Messaging.Protobuf;
using Google.Protobuf;
using GreenEnergyHub.Charges.Infrastructure.Messaging.Serialization;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;

namespace GreenEnergyHub.Charges.Infrastructure.Messaging.Registration
{
    public static class SimpleInjectorReceiveProtobufExtensions
    {
        private static readonly Type _mapperType = typeof(ProtobufInboundMapper<>);

        /// <summary>
        /// Configure the <see cref="IServiceCollection"/> to handle proto buf
        /// </summary>
        /// <param name="container">SimpleInjector container</param>
        /// <param name="configuration">Configuration of the parser</param>
        /// <typeparam name="TProtoContract">Protobuf contract</typeparam>
        /// <exception cref="ArgumentNullException"><paramref name="container"/> or <paramref name="configuration"/> is <c>null</c></exception>
        public static Container ReceiveProtobufMessage<TProtoContract>(
            this Container container,
            Action<ProtobufOneOfConfiguration<TProtoContract>> configuration)
            where TProtoContract : class, IMessage
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            var config = new ProtobufOneOfConfiguration<TProtoContract>();
            configuration.Invoke(config);

            container.Register<MessageExtractor<TProtoContract>>(Lifestyle.Scoped);
            container.Register<MessageDeserializer<TProtoContract>, ProtobufDeserializer<TProtoContract>>(Lifestyle.Scoped);

            var serviceDescriptors = ScanForMappers(container, typeof(TProtoContract).Assembly);
            container.Register(_mapperType, serviceDescriptors, Lifestyle.Scoped);

            container.Register(() => config.GetParser());

            return container;
        }

        private static IEnumerable<Type> ScanForMappers(Container container, Assembly assemblies)
        {
            return container.GetTypesToRegister(_mapperType, new List<Assembly> { assemblies }, new TypesToRegisterOptions
            {
                IncludeGenericTypeDefinitions = true,
                IncludeComposites = false,
            });
        }
    }
}
