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

using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands;
using GreenEnergyHub.Charges.TestCore.TestHelpers;

namespace GreenEnergyHub.Charges.TestCore.Builders.Command
{
    public class ChargeInformationCommandReceivedEventBuilder
    {
        private ChargeInformationCommand _command;

        public ChargeInformationCommandReceivedEventBuilder()
        {
            _command = new ChargeInformationCommandBuilder().Build();
        }

        public ChargeInformationCommandReceivedEventBuilder WithCommand(ChargeInformationCommand command)
        {
            _command = command;
            return this;
        }

        public ChargeInformationCommandReceivedEvent Build()
        {
            return new ChargeInformationCommandReceivedEvent(
                InstantHelper.GetTodayAtMidnightUtc(),
                _command);
        }
    }
}
