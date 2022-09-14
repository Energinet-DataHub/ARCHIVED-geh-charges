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

using GreenEnergyHub.Charges.Domain.Dtos.Messages.Command;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;

namespace GreenEnergyHub.Charges.Domain.Dtos.Validation
{
    /// <summary>
    /// Contract defining the input validator for change of charges messages.
    /// </summary>
    public interface IInputValidator<in TOperation>
        where TOperation : ChargeOperationDto
    {
        /// <summary>
        /// Input validation of operation/>.
        /// </summary>
        /// <param name="operation">The operation to validate.</param>
        /// <param name="document">Document information for operation to validate.</param>
        /// <returns>The validation result.</returns>
        ValidationResult Validate(TOperation operation, DocumentDto document);
    }
}
