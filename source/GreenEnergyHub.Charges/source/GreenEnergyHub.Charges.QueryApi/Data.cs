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

using System.Linq;
using GreenEnergyHub.Charges.QueryApi.Model;
using Microsoft.EntityFrameworkCore;

namespace GreenEnergyHub.Charges.QueryApi
{
    public class Data : IData
    {
        private readonly QueryDbContext _context;

        public Data(QueryDbContext context)
        {
            _context = context;
        }

        public IQueryable<ChargeLink> ChargeLinks => _context.ChargeLinks.AsNoTracking();

        public IQueryable<ChargeInformation> ChargeInformations => _context.ChargeInformations.AsNoTracking();

        public IQueryable<MeteringPoint> MeteringPoints => _context.MeteringPoints.AsNoTracking();

        public IQueryable<MarketParticipant> MarketParticipants => _context.MarketParticipants.AsNoTracking();

        public IQueryable<DefaultChargeLink> DefaultChargeLinks => _context.DefaultChargeLinks.AsNoTracking();

        public IQueryable<ChargePrice> ChargePrices => _context.ChargePrices.AsNoTracking();
    }
}
