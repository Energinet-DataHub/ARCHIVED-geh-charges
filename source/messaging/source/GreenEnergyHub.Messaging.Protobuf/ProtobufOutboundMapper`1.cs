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
using Google.Protobuf;
using GreenEnergyHub.Messaging.Transport;

namespace GreenEnergyHub.Messaging.Protobuf
{
    /// <summary>
    /// Generic proto buf mapper
    /// </summary>
    /// <typeparam name="T">Proto buf contract</typeparam>
    public abstract class ProtobufOutboundMapper<T> : ProtobufOutboundMapper
        where T : IOutboundMessage
    {
        /// <summary>
        /// Convert an <see cref="IOutboundMessage"/> to proto buf contract
        /// </summary>
        /// <param name="obj">Message to map</param>
        /// <returns>Converted message</returns>
        /// <exception cref="InvalidOperationException"><paramref name="obj"/> is not of <typeparamref name="T"> type</typeparamref></exception>
        public override IMessage Convert(IOutboundMessage obj)
        {
            if (obj is T outboundMessage) return Convert(outboundMessage);

            throw new InvalidOperationException();
        }

        /// <summary>
        /// Convert to a proto buf contract
        /// </summary>
        /// <param name="obj">Application object to map</param>
        /// <returns>Converted proto buf contract</returns>
        protected abstract IMessage Convert(T obj);
    }
}
