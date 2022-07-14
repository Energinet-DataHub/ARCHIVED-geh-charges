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
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;

namespace GreenEnergyHub.Charges.Application.Charges.Acknowledgement
{
    public interface IChargeCommandReceiptService
    {
        Task RejectAsync(ChargeInformationCommand command, ValidationResult validationResult);

        Task AcceptAsync(ChargeInformationCommand command);

        /// <summary>
        /// Send reject receipt containing all invalid operations
        /// </summary>
        /// <param name="operationsToBeRejected"></param>
        /// <param name="document"></param>
        /// <param name="rejectionRules"></param>
        Task RejectInvalidOperationsAsync(
            IReadOnlyCollection<ChargeOperationDto> operationsToBeRejected,
            DocumentDto document,
            IList<IValidationRuleContainer> rejectionRules);

        /// <summary>
        /// Send accept receipt containing all valid operations
        /// </summary>
        /// <param name="operationsToBeConfirmed"></param>
        /// <param name="document"></param>
        Task AcceptValidOperationsAsync(
            IReadOnlyCollection<ChargeOperationDto> operationsToBeConfirmed,
            DocumentDto document);
    }
}
