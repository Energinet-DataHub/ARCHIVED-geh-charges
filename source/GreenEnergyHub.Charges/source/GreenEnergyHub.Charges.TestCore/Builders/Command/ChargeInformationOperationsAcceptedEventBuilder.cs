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
using GreenEnergyHub.Charges.Domain.Dtos.Events;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using NodaTime;

namespace GreenEnergyHub.Charges.TestCore.Builders.Command
{
    public class ChargeInformationOperationsAcceptedEventBuilder
    {
        private Instant _publishedTime;
        private DocumentDto _document;
        private IReadOnlyCollection<ChargeInformationOperationDto> _operations;

        public ChargeInformationOperationsAcceptedEventBuilder()
        {
            _publishedTime = SystemClock.Instance.GetCurrentInstant();
            _document = new DocumentDtoBuilder()
                .WithDocumentType(DocumentType.ConfirmRequestChangeOfPriceList)
                .WithBusinessReasonCode(BusinessReasonCode.UpdateChargeInformation)
                .Build();
            _operations = new List<ChargeInformationOperationDto>() { new ChargeInformationOperationDtoBuilder().Build() };
        }

        public ChargeInformationOperationsAcceptedEventBuilder WithDocument(DocumentDto documentDto)
        {
            _document = documentDto;
            return this;
        }

        public ChargeInformationOperationsAcceptedEventBuilder WithOperations(IReadOnlyCollection<ChargeInformationOperationDto> operations)
        {
            _operations = operations;
            return this;
        }

        public ChargeInformationOperationsAcceptedEvent Build()
        {
            return new ChargeInformationOperationsAcceptedEvent(_publishedTime, _document, _operations);
        }
    }
}
