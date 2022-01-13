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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands;
using GreenEnergyHub.Charges.Domain.Dtos.Messages.Events;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using NodaTime;

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksRejectionEvents
{
    public class ChargeLinksRejectedEvent : InternalEventBase
    {
        public ChargeLinksRejectedEvent(
            Instant publishedTime,
            ChargeLinksCommand chargeLinksCommand,
            IEnumerable<ValidationError> validationErrors)
            : base(publishedTime)
        {
            ChargeLinksCommand = chargeLinksCommand;
            ValidationErrors = validationErrors;
        }

        public ChargeLinksCommand ChargeLinksCommand { get; }

        public IEnumerable<ValidationError> ValidationErrors { get; }
    }
}
