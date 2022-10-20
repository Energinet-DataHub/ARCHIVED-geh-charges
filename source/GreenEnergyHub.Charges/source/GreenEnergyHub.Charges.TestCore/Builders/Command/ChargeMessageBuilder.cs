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

namespace GreenEnergyHub.Charges.TestCore.Builders.Command
{
    public class ChargeMessageBuilder
    {
        private static Guid _chargeId = Guid.NewGuid();
        private static string _messageId = "messageId";

        public ChargeMessageBuilder WithChargeId(Guid chargeId)
        {
            _chargeId = chargeId;
            return this;
        }

        public ChargeMessageBuilder WithMessageId(string messageId)
        {
            _messageId = messageId;
            return this;
        }

        public ChargeMessage Build()
        {
            return ChargeMessage.Create(_chargeId, _messageId);
        }
    }
}
