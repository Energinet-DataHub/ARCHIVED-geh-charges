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

namespace GreenEnergyHub.Charges.Domain.Charges
{
    /// <summary>
    /// Class is used for handling messages related to a charge.
    /// </summary>
    public class ChargeMessage
    {
        private ChargeMessage(Guid chargeId, string messageId)
        {
            Id = Guid.NewGuid();
            ChargeId = chargeId;
            MessageId = messageId;
        }

        /// <summary>
        /// Minimal ctor to support EF Core.
        /// </summary>
        // ReSharper disable once UnusedMember.Local - used by EF Core
        private ChargeMessage()
        {
            ChargeId = Guid.Empty;
            MessageId = string.Empty;
        }

        /// <summary>
        /// Globally unique identifier of the charge message reference.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// The charge id
        /// </summary>
        public Guid ChargeId { get; }

        /// <summary>
        /// The message id
        /// </summary>
        public string MessageId { get; }

        public static ChargeMessage Create(Guid chargeId, string messageId)
        {
            ArgumentNullException.ThrowIfNull(chargeId);
            ArgumentNullException.ThrowIfNull(messageId);

            return new ChargeMessage(chargeId, messageId);
        }
    }
}
