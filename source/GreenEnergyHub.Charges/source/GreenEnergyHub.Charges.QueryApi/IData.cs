// <copyright file="IQueryables.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Linq;
using GreenEnergyHub.Charges.QueryApi.Model.Scaffolded;

namespace GreenEnergyHub.Charges.QueryApi
{
    public interface IData
    {
        public IQueryable<ChargeLink> ChargeLinks { get; }

        public IQueryable<Charge> Charges { get; }

        public IQueryable<MeteringPoint> MeteringPoints { get; }
    }
}
