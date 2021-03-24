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
    /// Convert a proto buf contract
    /// </summary>
    /// <typeparam name="TMessage">Proto buf contract</typeparam>
    public abstract class ProtobufInboundMapper<TMessage> : ProtobufInboundMapper
        where TMessage : IMessage
    {
        /// <summary>
        /// Convert an <see cref="IMessage"/> to <see cref="IInboundMessage"/>
        /// </summary>
        /// <param name="obj">Proto buf payload</param>
        /// <returns>The converted application message</returns>
        /// <exception cref="InvalidOperationException">If <paramref name="obj"></paramref> is not of <typeparamref name="TMessage"></typeparamref> type</exception>
        public override IInboundMessage Convert(IMessage obj)
        {
            if (obj is TMessage outboundMessage) return Convert(outboundMessage);

            throw new InvalidOperationException();
        }

        /// <summary>
        /// Convert proto buf contract
        /// </summary>
        /// <param name="obj">Contract to convert</param>
        /// <returns>Application message</returns>
        protected abstract IInboundMessage Convert(TMessage obj);
    }
}
