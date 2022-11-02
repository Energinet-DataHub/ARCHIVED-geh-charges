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

using System.Linq;
using GreenEnergyHub.Charges.QueryApi.Model;
using Charge = GreenEnergyHub.Charges.QueryApi.Model.Charge;
using ChargeMessage = GreenEnergyHub.Charges.QueryApi.Model.ChargeMessage;

namespace GreenEnergyHub.Charges.QueryApi
{
    public interface IData
    {
        public IQueryable<ChargeLink> ChargeLinks { get; }

        public IQueryable<Charge> Charges { get; }

        public IQueryable<ChargePoint> ChargePoints { get; }

        public IQueryable<ChargeMessage> ChargeMessages { get; }

        public IQueryable<MeteringPoint> MeteringPoints { get; }

        public IQueryable<MarketParticipant> MarketParticipants { get; }

        public IQueryable<DefaultChargeLink> DefaultChargeLinks { get; }
    }
}
