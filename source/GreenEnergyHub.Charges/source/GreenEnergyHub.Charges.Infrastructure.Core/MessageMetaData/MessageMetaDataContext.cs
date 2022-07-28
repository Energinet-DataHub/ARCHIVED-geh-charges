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
using GreenEnergyHub.Charges.MessageHub.AvailableData.Messaging;
using NodaTime;

namespace GreenEnergyHub.Charges.Infrastructure.Core.MessageMetaData
{
    public class MessageMetaDataContext : IMessageMetaDataContext
    {
        private string? _replyTo;
        private string? _sessionId;

        public MessageMetaDataContext(IClock clock)
        {
            // TODO: This must be replaced by the actual request data time carried in some meta data of the message
            RequestDataTime = clock.GetCurrentInstant();
        }

        public Instant RequestDataTime { get; }

        public string ReplyTo
        {
            get
            {
                if (string.IsNullOrEmpty(_replyTo))
                    throw new InvalidOperationException($"Property '{nameof(ReplyTo)}' is missing");

                return _replyTo;
            }
        }

        public string SessionId
        {
            get
            {
                if (string.IsNullOrEmpty(_sessionId))
                    throw new InvalidOperationException($"Property '{nameof(SessionId)}' is missing");

                return _sessionId;
            }
        }

        public bool IsReplyToSet()
        {
            return !string.IsNullOrEmpty(_replyTo);
        }

        public void SetReplyTo(string? messageType)
        {
            _replyTo = messageType;
        }

        public void SetSessionId(string? sessionId)
        {
            _sessionId = sessionId;
        }
    }
}
