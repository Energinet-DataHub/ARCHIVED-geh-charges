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

using System.Collections.Generic;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;

namespace GreenEnergyHub.Charges.Tests.Builders.Application
{
    public class ChargeBundleDtoBuilder
    {
        private readonly List<ChargeInformationCommand> _commands = new();
        private DocumentDto _document = new();

        public ChargeBundleDtoBuilder WithDocument(DocumentDto document)
        {
            _document = document;
            return this;
        }

        public ChargeBundleDtoBuilder WithCommand(ChargeInformationCommand command)
        {
            _commands.Add(command);
            return this;
        }

        public ChargeBundleDto Build()
        {
            return new ChargeBundleDto(_document, _commands);
        }
    }
}
