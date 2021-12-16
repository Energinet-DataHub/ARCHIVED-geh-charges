// <copyright file="ChargeQueryLogic.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Linq;
using GreenEnergyHub.Charges.QueryApi.Model.Scaffolded;

namespace GreenEnergyHub.Charges.QueryApi.QueryPredicates
{
    public static class ChargeQueryLogic
    {
        public static IQueryable<ChargeLink> ForMeteringPoint(this IQueryable<ChargeLink> queryable, string meteringPointId)
        {
            return queryable.Where(c => c.MeteringPoint.MeteringPointId == meteringPointId);
        }
    }
}
