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

using Energinet.DataHub.Charges.Contracts.Charge;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands;

namespace GreenEnergyHub.Charges.WebApi.Factories;

/// <summary>
/// Factory for creating <see cref="ChargeInformationCommand"/>
/// </summary>
public interface IChargeInformationCommandFactory
{
    /// <summary>
    /// Factory for creating <see cref="ChargeInformationCommand"/> from <see cref="CreateChargeV1Dto"/>
    /// </summary>
    /// <param name="createChargeV1Dto"></param>
    /// <returns>A <see cref="ChargeInformationCommand"/></returns>
    ChargeInformationCommand Create(CreateChargeV1Dto createChargeV1Dto);
}
