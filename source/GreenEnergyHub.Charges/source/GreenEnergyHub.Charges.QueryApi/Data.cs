// <copyright file="Queryables.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Linq;
using GreenEnergyHub.Charges.QueryApi.Model.Scaffolded;
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

        public IQueryable<Charge> Charges => _context.Charges.AsNoTracking();

        public IQueryable<MeteringPoint> MeteringPoints => _context.MeteringPoints.AsNoTracking();
    }
}
