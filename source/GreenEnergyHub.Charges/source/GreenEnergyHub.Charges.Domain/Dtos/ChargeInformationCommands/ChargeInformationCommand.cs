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
using GreenEnergyHub.Charges.Domain.Dtos.Messages.Command;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands
{
    // Non-nullable member is uninitialized is ignored
    // Only properties which is allowed to be null is nullable
    // ChargeCommand integrity is null checked by ChargeCommandNullChecker
    public class ChargeInformationCommand : CommandBase
    {
        public ChargeInformationCommand(DocumentDto document, IReadOnlyCollection<ChargeInformationOperationDto> operations)
        {
            Document = document;
            Operations = operations;
        }

        public override DocumentDto Document { get; }

        public override IReadOnlyCollection<ChargeInformationOperationDto> Operations { get; }
    }
}
