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
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.MarketParticipants;

namespace GreenEnergyHub.Charges.TestCore.Builders.Testables
{
    /// <summary>
    /// Makes it easier to auto-mock grid access providers in tests.
    /// </summary>
    public class TestEnergySupplier : MarketParticipant
    {
        public TestEnergySupplier(string marketParticipantId)
            : base(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), marketParticipantId, MarketParticipantStatus.Active, MarketParticipantRole.EnergySupplier)
        {
        }
    }
}
