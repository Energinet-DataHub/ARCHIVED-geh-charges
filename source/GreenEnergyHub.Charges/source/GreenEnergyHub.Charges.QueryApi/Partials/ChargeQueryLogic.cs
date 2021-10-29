// <copyright file="ChargeQueryLogic.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Linq;

namespace GreenEnergyHub.Charges.QueryApi.ScaffoldedModels
{
    public static class ChargeQueryLogic
    {
        public static IQueryable<ChargeLink> ForMeteringPoint(this IQueryable<ChargeLink> queryable, string meteringPointId)
        {
            return queryable.Where(c => c.MeteringPoint.MeteringPointId == meteringPointId);
        }
    }
}
