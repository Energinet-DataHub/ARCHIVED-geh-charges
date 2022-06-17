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

using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandAcceptedEvents;

using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands;
using NodaTime;

namespace GreenEnergyHub.Charges.Tests.Builders.Command
{
    public class ChargeCommandAcceptedEventBuilder
    {
        private Instant _publishedTime;
        private ChargeInformationCommand _chargeInformationCommand;

        public ChargeCommandAcceptedEventBuilder()
        {
            _publishedTime = SystemClock.Instance.GetCurrentInstant();
            _chargeInformationCommand = new ChargeCommandBuilder().Build();
        }

        public ChargeCommandAcceptedEventBuilder WithPublishedTime(Instant publishedTime)
        {
            _publishedTime = publishedTime;
            return this;
        }

        public ChargeCommandAcceptedEventBuilder WithChargeCommand(ChargeInformationCommand chargeInformationCommand)
        {
            _chargeInformationCommand = chargeInformationCommand;
            return this;
        }

        public ChargeCommandAcceptedEvent Build()
        {
            return new ChargeCommandAcceptedEvent(_publishedTime, _chargeInformationCommand);
        }
    }
}
