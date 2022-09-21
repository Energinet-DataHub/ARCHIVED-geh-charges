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
using System.Linq;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands;
using GreenEnergyHub.Charges.Domain.Dtos.Events;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using NodaTime;

namespace GreenEnergyHub.Charges.TestCore.Builders.Command
{
    public class ChargeInformationOperationsRejectedEventBuilder
    {
        private Instant _publishedTime;
        private DocumentDto _document;
        private IReadOnlyCollection<ChargeInformationOperationDto> _operations;
        private IEnumerable<ValidationError> _validationErrors;

        public ChargeInformationOperationsRejectedEventBuilder()
        {
            _publishedTime = SystemClock.Instance.GetCurrentInstant();
            _document = new DocumentDtoBuilder()
                .WithBusinessReasonCode(BusinessReasonCode.UpdateChargeInformation)
                .WithDocumentType(DocumentType.RejectRequestChangeOfPriceList)
                .Build();
            _operations = new List<ChargeInformationOperationDto>() { new ChargeInformationOperationDtoBuilder().Build() };
            _validationErrors = new List<ValidationError>();
        }

        public ChargeInformationOperationsRejectedEventBuilder WithOperations(List<ChargeInformationOperationDto> operations)
        {
            _operations = operations;
            return this;
        }

        public ChargeInformationOperationsRejectedEventBuilder WithValidationErrors(IEnumerable<ValidationError> validationErrors)
        {
            _validationErrors = validationErrors;
            return this;
        }

        public ChargeInformationOperationsRejectedEvent Build()
        {
            if (!_validationErrors.Any())
            {
                _validationErrors = new List<ValidationError>
                {
                    new ValidationError(
                        ValidationRuleIdentifier.MaximumPrice,
                        _operations.First().OperationId,
                        string.Empty),
                };
            }

            return new ChargeInformationOperationsRejectedEvent(_publishedTime, _document, _operations, _validationErrors);
        }
    }
}
