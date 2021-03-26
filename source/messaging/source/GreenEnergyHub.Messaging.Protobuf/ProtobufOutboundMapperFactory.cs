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
using GreenEnergyHub.Messaging.Transport;

namespace GreenEnergyHub.Messaging.Protobuf
{
    /// <summary>
    /// Helper class to locate mapper
    /// </summary>
    public sealed class ProtobufOutboundMapperFactory
    {
        private static readonly Type _protobufMapperType = typeof(ProtobufOutboundMapper<>);
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Create a mapper factory
        /// </summary>
        /// <param name="serviceProvider">Current service provider</param>
        public ProtobufOutboundMapperFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Find a mapper for a type
        /// </summary>
        /// <param name="message">Message to locate mapper for</param>
        /// <returns>Located mapper</returns>
        /// <exception cref="ArgumentNullException"><paramref name="message"/> is <c>null</c></exception>
        /// <exception cref="InvalidOperationException">no mapper found</exception>
        public ProtobufOutboundMapper GetMapper(IOutboundMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            var typeToLocate = _protobufMapperType.MakeGenericType(message.GetType());
            var mapper = _serviceProvider.GetService(typeToLocate) as ProtobufOutboundMapper;

            return mapper ?? throw new InvalidOperationException("Mapper not found");
        }
    }
}
