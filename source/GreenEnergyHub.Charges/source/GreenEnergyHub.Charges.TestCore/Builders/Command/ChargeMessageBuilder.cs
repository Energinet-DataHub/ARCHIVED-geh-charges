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
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.TestCore.TestHelpers;
using NodaTime;

namespace GreenEnergyHub.Charges.TestCore.Builders.Command
{
    public class ChargeMessageBuilder
    {
        private static string _senderProvidedChargeId = Guid.NewGuid().ToString();
        private static ChargeType _chargeType = ChargeType.Unknown;
        private static string _marketParticipantId = Guid.NewGuid().ToString();
        private static string _messageId = "messageId";
        private static DocumentType _messageType = DocumentType.RequestChangeOfPriceList;
        private static Instant _messageDateTime = InstantHelper.GetTodayAtMidnightUtc();

        public ChargeMessageBuilder WithSenderProvidedChargeId(string senderProvidedChargeId)
        {
            _senderProvidedChargeId = senderProvidedChargeId;
            return this;
        }

        public ChargeMessageBuilder WithChargeType(ChargeType chargeType)
        {
            _chargeType = chargeType;
            return this;
        }

        public ChargeMessageBuilder WithMarketParticipantId(string marketParticipantId)
        {
            _marketParticipantId = marketParticipantId;
            return this;
        }

        public ChargeMessageBuilder WithMessageId(string messageId)
        {
            _messageId = messageId;
            return this;
        }

        public ChargeMessage Build()
        {
            return ChargeMessage.Create(
                _senderProvidedChargeId,
                _chargeType,
                _marketParticipantId,
                _messageId,
                _messageType,
                _messageDateTime);
        }
    }
}
