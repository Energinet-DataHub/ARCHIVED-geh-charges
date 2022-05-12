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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GreenEnergyHub.Charges.Domain.ChargeInformations
{
    /// <summary>
    /// Contract defining the capabilities of the infrastructure component facilitating interaction with the charges data store.
    /// </summary>
    public interface IChargeInformationRepository
    {
        Task AddAsync(ChargeInformation chargeInformation);

        Task<ChargeInformation> GetAsync(ChargeInformationIdentifier chargeInformationIdentifier);

        Task<ChargeInformation> GetAsync(Guid id);

        Task<IReadOnlyCollection<ChargeInformation>> GetAsync(IReadOnlyCollection<Guid> ids);

        Task<ChargeInformation?> GetOrNullAsync(ChargeInformationIdentifier chargeInformationIdentifier);
    }
}
