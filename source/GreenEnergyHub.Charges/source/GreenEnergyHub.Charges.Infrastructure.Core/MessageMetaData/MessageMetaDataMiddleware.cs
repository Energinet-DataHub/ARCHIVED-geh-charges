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

using System.Collections.Generic;
using System.Threading.Tasks;
using Energinet.DataHub.Core.JsonSerialization;
using GreenEnergyHub.Charges.Application.Messaging;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;

namespace GreenEnergyHub.Charges.Infrastructure.Core.MessageMetaData
{
    public class MessageMetaDataMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly IMessageMetaDataContext _messageMetaDataContext;
        private readonly IJsonSerializer _jsonSerializer;

        public MessageMetaDataMiddleware(IMessageMetaDataContext messageMetaDataContext, IJsonSerializer jsonSerializer)
        {
            _messageMetaDataContext = messageMetaDataContext;
            _jsonSerializer = jsonSerializer;
        }

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            var sessionId = GetSessionId(context);
            var messageMetaData = GetMessageMetaData(context);
            ((MessageMetaDataContext)_messageMetaDataContext).SetReplyTo(messageMetaData.ReplyTo);
            ((MessageMetaDataContext)_messageMetaDataContext).SetSessionId(sessionId);

            await next(context).ConfigureAwait(false);
        }

        private static string? GetSessionId(FunctionContext context)
        {
            context.BindingContext.BindingData.TryGetValue("SessionId", out var session);
            return (string?)session;
        }

        private MessageMetadata GetMessageMetaData(FunctionContext context)
        {
            context.BindingContext.BindingData.TryGetValue("UserProperties", out var metadata);

            if (metadata != null)
            {
                var eventMetadata = _jsonSerializer.Deserialize<MessageMetadata>(metadata.ToString()!);
                return eventMetadata;
            }

            return new MessageMetadata();
        }
    }
}
