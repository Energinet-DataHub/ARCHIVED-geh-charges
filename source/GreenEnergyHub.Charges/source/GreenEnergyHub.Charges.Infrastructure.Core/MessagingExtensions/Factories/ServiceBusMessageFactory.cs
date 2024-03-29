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

using System;
using System.Collections.Generic;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;
using GreenEnergyHub.Charges.Application.Messaging;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Infrastructure.Core.MessageMetaData;

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

        public ServiceBusMessage CreateInternalMessage(string data, string messageType)
        {
            var serviceBusMessage = CreateServiceBusMessage(data, messageVersion: 1, messageType);

            if (_messageMetaDataContext.IsReplyToSet())
            {
                serviceBusMessage.ApplicationProperties.Add(
                    new KeyValuePair<string, object>(MessageMetaDataConstants.ReplyTo, _messageMetaDataContext.ReplyTo));
            }

            return serviceBusMessage;
        }

        public ServiceBusMessage CreateExternalMessage(byte[] data, string messageType)
        {
            return new ServiceBusMessage(data)
            {
                CorrelationId = _correlationContext.Id,
                ApplicationProperties =
                {
                    new KeyValuePair<string, object>(
                        MessageMetaDataConstants.OperationTimestamp,
                        _messageMetaDataContext.RequestDataTime.GetCreatedDateTimeFormat()),
                    new KeyValuePair<string, object>(MessageMetaDataConstants.CorrelationId, _correlationContext.Id),
                    new KeyValuePair<string, object>(MessageMetaDataConstants.MessageVersion, 1),
                    new KeyValuePair<string, object>(MessageMetaDataConstants.MessageType, messageType),
                    new KeyValuePair<string, object>(MessageMetaDataConstants.EventIdentification, Guid.NewGuid()),
                },
            };
        }

        private ServiceBusMessage CreateServiceBusMessage(string data, int messageVersion, string messageType)
        {
            return new ServiceBusMessage(data)
            {
                Subject = messageType,
                CorrelationId = _correlationContext.Id,
                ApplicationProperties =
                {
                    new KeyValuePair<string, object>(
                        MessageMetaDataConstants.OperationTimestamp,
                        _messageMetaDataContext.RequestDataTime.GetCreatedDateTimeFormat()),
                    new KeyValuePair<string, object>(MessageMetaDataConstants.CorrelationId, _correlationContext.Id),
                    new KeyValuePair<string, object>(MessageMetaDataConstants.MessageVersion, messageVersion),
                    new KeyValuePair<string, object>(MessageMetaDataConstants.MessageType, messageType),
                    new KeyValuePair<string, object>(MessageMetaDataConstants.EventIdentification, Guid.NewGuid()),
                },
            };
        }
    }
}
