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
using GreenEnergyHub.Charges.Infrastructure.Outbox;
using GreenEnergyHub.Charges.TestCore;
using NodaTime;

namespace GreenEnergyHub.Charges.Tests.Builders.Command
{
    public class OutboxMessageBuilder
    {
        private Instant _creationDate = InstantHelper.GetTodayAtMidnightUtc();
        private string _data = string.Empty;
        private string _type = string.Empty;

        public OutboxMessageBuilder WithData(string data)
        {
            _data = data;
            return this;
        }

        public OutboxMessageBuilder WithType(string type)
        {
            _type = type;
            return this;
        }

        public OutboxMessageBuilder WithCreationDate(Instant creationDate)
        {
            _creationDate = creationDate;
            return this;
        }

        public OutboxMessage Build()
        {
            var outboxMessage = OutboxMessage.Create(_data, Guid.NewGuid().ToString(), _type, _creationDate);
            return outboxMessage;
        }
    }
}
