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
using GreenEnergyHub.Charges.Domain.Dtos.Messages.Command;
using GreenEnergyHub.Charges.Domain.Dtos.Messages.Events;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using NodaTime;

namespace GreenEnergyHub.Charges.Application.Charges.Events
{
    public class PriceRejectedEvent : DomainEvent
    {
        public PriceRejectedEvent(
            Instant publishedTime,
            DocumentDto document,
            IReadOnlyCollection<ChargePriceOperationDto> operations,
            IEnumerable<ValidationError> validationErrors)
            : base(publishedTime)
        {
            Document = document;
            Operations = operations;
            ValidationErrors = validationErrors;
        }

        public DocumentDto Document { get; }

        public IReadOnlyCollection<ChargePriceOperationDto> Operations { get; }

        public IEnumerable<ValidationError> ValidationErrors { get; }
    }
}
