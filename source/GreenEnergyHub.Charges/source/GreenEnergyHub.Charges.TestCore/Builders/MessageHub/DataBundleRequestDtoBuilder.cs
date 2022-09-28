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
using Energinet.DataHub.MessageHub.Model.Model;

namespace GreenEnergyHub.Charges.TestCore.Builders.MessageHub
{
    public class DataBundleRequestDtoBuilder
    {
        private Guid _requestId;
        private string _availableDataReferenceId;
        private string _idempotencyId;
        private MessageTypeDto _messageType;
        private ResponseFormat _responseFormat;
        private double _responseVersion;

        public DataBundleRequestDtoBuilder()
        {
            _requestId = Guid.NewGuid();
            _availableDataReferenceId = Guid.NewGuid().ToString();
            _idempotencyId = Guid.NewGuid().ToString();
            _messageType = new MessageTypeDto("messageType");
            _responseFormat = ResponseFormat.Xml;
            _responseVersion = 1.0;
        }

        public DataBundleRequestDtoBuilder WithResponseFormat(ResponseFormat responseFormat)
        {
            _responseFormat = responseFormat;
            return this;
        }

        public DataBundleRequestDto Build()
        {
            return new DataBundleRequestDto(
                _requestId,
                _availableDataReferenceId,
                _idempotencyId,
                _messageType,
                _responseFormat,
                _responseVersion);
        }
    }
}
