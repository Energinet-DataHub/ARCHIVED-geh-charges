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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;

namespace GreenEnergyHub.Charges.Tests.Builders.Command
{
   public class ChargeCommandBuilder
    {
        private List<ChargeInformationDto> _chargeOperationDtos;
        private DocumentDto _documentDto;

        public ChargeCommandBuilder()
        {
            _chargeOperationDtos = new List<ChargeInformationDto> { new ChargeOperationDtoBuilder().Build() };
            _documentDto = new DocumentDtoBuilder().Build();
        }

        public ChargeCommandBuilder WithDocumentDto(DocumentDto documentDto)
        {
            _documentDto = documentDto;
            return this;
        }

        public ChargeCommandBuilder WithChargeOperation(ChargeInformationDto chargeInformationDto)
        {
            _chargeOperationDtos.Clear();
            _chargeOperationDtos.Add(chargeInformationDto);
            return this;
        }

        public ChargeCommandBuilder WithChargeOperations(List<ChargeInformationDto> chargeOperationDtos)
        {
            _chargeOperationDtos = chargeOperationDtos;
            return this;
        }

        public ChargeCommand Build()
        {
            return new ChargeCommand(_documentDto, _chargeOperationDtos);
        }
    }
}
