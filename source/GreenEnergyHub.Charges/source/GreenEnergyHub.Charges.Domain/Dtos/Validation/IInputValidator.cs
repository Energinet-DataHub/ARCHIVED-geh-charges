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

namespace GreenEnergyHub.Charges.Domain.Dtos.Validation
{
    /// <summary>
    /// Contract defining the input validator for change of charges messages.
    /// </summary>
    public interface IInputValidator<in TCommand, in TOperation>
        where TCommand : CommandBase
        where TOperation : OperationBase
    {
        /// <summary>
        /// Input validation of command/>.
        /// </summary>
        /// <param name="command">The message to validate.</param>
        /// <returns>The validation result.</returns>
        ValidationResult Validate(TCommand command);

        /// <summary>
        /// Input validation of operation/>.
        /// </summary>
        /// <param name="operation">The operation to validate.</param>
        /// <returns>The validation result.</returns>
        ValidationResult Validate(TOperation operation);
    }
}
