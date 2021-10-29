// <copyright file="Queryables.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Linq;
using GreenEnergyHub.Charges.QueryApi.ScaffoldedModels;
using Microsoft.EntityFrameworkCore;

namespace GreenEnergyHub.Charges.QueryApi
{
    public class Data : IData
    {
        private readonly ChargesdatabaseContext _context;

        public Data(ChargesdatabaseContext context)
        {
            _context = context;
        }

        public IQueryable<ChargeLink> ChargeLinks => _context.ChargeLink.AsNoTracking();

        public IQueryable<Charge> Charges => _context.Charge.AsNoTracking();

        public IQueryable<MeteringPoint> MeteringPoints => _context.MeteringPoint.AsNoTracking();
    }
}
