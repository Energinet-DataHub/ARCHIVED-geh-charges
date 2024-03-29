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

namespace GreenEnergyHub.Charges.Domain.Dtos.Validation
{
    /// <summary>
    /// Interface for validationrules in the Charges domain
    /// </summary>
    public interface IValidationRule
    {
        /// <summary>
        /// Validity of the ChargeCommand given the current rule
        /// </summary>
        public bool IsValid { get; }

        /// <summary>
        /// Identifier of the current rule
        /// </summary>
        public ValidationRuleIdentifier ValidationRuleIdentifier { get; }
    }
}
