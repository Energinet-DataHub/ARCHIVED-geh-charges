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
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;

namespace GreenEnergyHub.Charges.TestCore.Builders.Command
{
   public class ChargePriceCommandBuilder
    {
        private List<ChargePriceOperationDto> _chargePriceOperationDtos;
        private DocumentDto _documentDto;

        public ChargePriceCommandBuilder()
        {
            _chargePriceOperationDtos = new List<ChargePriceOperationDto> { new ChargePriceOperationDtoBuilder().Build() };
            _documentDto = new DocumentDtoBuilder().Build();
        }

        public ChargePriceCommandBuilder WithChargeOperation(ChargePriceOperationDto chargePriceOperationDto)
        {
            _chargePriceOperationDtos.Clear();
            _chargePriceOperationDtos.Add(chargePriceOperationDto);
            return this;
        }

        public ChargePriceCommandBuilder WithChargeOperations(List<ChargePriceOperationDto> chargePriceOperationDtos)
        {
            _chargePriceOperationDtos = chargePriceOperationDtos;
            return this;
        }

        public ChargePriceCommandBuilder WithDocument(DocumentDto document)
        {
            _documentDto = document;
            return this;
        }

        public ChargePriceCommand Build()
        {
            return new ChargePriceCommand(_documentDto, _chargePriceOperationDtos);
        }
    }
}
