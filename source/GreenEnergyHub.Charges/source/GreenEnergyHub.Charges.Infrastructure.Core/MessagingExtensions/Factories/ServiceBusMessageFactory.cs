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

using System.Collections.Generic;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;
using GreenEnergyHub.Charges.Application.Messaging;

namespace GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions.Factories
{
    public class ServiceBusMessageFactory : IServiceBusMessageFactory
    {
        private readonly ICorrelationContext _correlationContext;
        private readonly IMessageMetaDataContext _messageMetaDataContext;

        public ServiceBusMessageFactory(
            ICorrelationContext correlationContext,
            IMessageMetaDataContext messageMetaDataContext)
        {
            _correlationContext = correlationContext;
            _messageMetaDataContext = messageMetaDataContext;
        }

        public ServiceBusMessage CreateInternalMessage(string data)
        {
            if (_messageMetaDataContext.IsReplyToSet())
            {
                return new ServiceBusMessage(data)
                {
                    CorrelationId = _correlationContext.Id,
                    ApplicationProperties =
                    {
                        new KeyValuePair<string, object>("ReplyTo", _messageMetaDataContext.ReplyTo),
                        new KeyValuePair<string, object>("OperationCorrelationId", _correlationContext.Id),
                        // Todo: Integration test, set:
                        // MessageTypeName = "
                        // MessageVersionName
                        // TimeStampName = "Op
                        // CorrelationIdName =
                        // EventIdentifierName
                    },
                };
            }

            return new ServiceBusMessage(data)
            {
                CorrelationId = _correlationContext.Id,
                ApplicationProperties =
                {
                    new KeyValuePair<string, object>("OperationCorrelationId", _correlationContext.Id),
                },
            };
        }

        public ServiceBusMessage CreateExternalMessage(byte[] data)
        {
            return new ServiceBusMessage(data) { CorrelationId = _correlationContext.Id, };
        }
    }
}
