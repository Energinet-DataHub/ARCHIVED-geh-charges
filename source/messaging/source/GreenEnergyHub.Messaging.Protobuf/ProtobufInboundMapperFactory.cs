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

namespace GreenEnergyHub.Messaging.Protobuf
{
    /// <summary>
    /// Helper class to find a registered map
    /// </summary>
    public sealed class ProtobufInboundMapperFactory
    {
        private static readonly Type _protobufReverseMapperType = typeof(ProtobufInboundMapper<>);
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Create a Mapper factory
        /// </summary>
        /// <param name="serviceProvider">The current service provider</param>
        public ProtobufInboundMapperFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Get a <see cref="ProtobufInboundMapper"/> from the DI system
        /// </summary>
        /// <param name="typeOfMessage">Message type to locate mapper for</param>
        /// <returns>Mapper for the type</returns>
        /// <exception cref="ArgumentNullException"><paramref name="typeOfMessage"></paramref> is <c>null</c></exception>
        /// <exception cref="InvalidOperationException">no mapper is found for <paramref name="typeOfMessage"/></exception>
        public ProtobufInboundMapper GetMapper(Type typeOfMessage)
        {
            if (typeOfMessage == null) throw new ArgumentNullException(nameof(typeOfMessage));

            var typeToLocate = _protobufReverseMapperType.MakeGenericType(typeOfMessage);
            var mapper = _serviceProvider.GetService(typeToLocate) as ProtobufInboundMapper;

            return mapper ?? throw new InvalidOperationException("Mapper not found");
        }
    }
}
