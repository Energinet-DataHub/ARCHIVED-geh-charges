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

using System.Collections.Generic;
using System.Linq;
using GreenEnergyHub.Charges.Application.Charges.Events;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.TestCore;
using NodaTime;

namespace GreenEnergyHub.Charges.Tests.Builders.Command
{
    public class PriceConfirmedEventBuilder
    {
        private Instant _publishedTime;
        private DocumentDto _document;
        private IReadOnlyCollection<ChargePriceOperationDto> _operations;

        public PriceConfirmedEventBuilder()
        {
            _publishedTime = SystemClock.Instance.GetCurrentInstant();
            _document = new DocumentDtoBuilder().WithDocumentType(DocumentType.ConfirmRequestChangeOfPriceList).Build();
            _operations = new List<ChargePriceOperationDto>() { new ChargePriceOperationDtoBuilder().Build() };
        }

        public PriceConfirmedEventBuilder WithOperations(List<ChargePriceOperationDto> operations)
        {
            _operations = operations;
            return this;
        }

        public PriceConfirmedEvent Build()
        {
            return new PriceConfirmedEvent(_publishedTime, _document, _operations);
        }
    }
}