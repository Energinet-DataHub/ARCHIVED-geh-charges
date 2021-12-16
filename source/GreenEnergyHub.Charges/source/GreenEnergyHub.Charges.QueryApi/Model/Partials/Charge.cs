// <copyright file="Charge.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using GreenEnergyHub.Charges.Domain.Charges;

namespace GreenEnergyHub.Charges.QueryApi.Model.Scaffolded
{
    /// <summary>
    /// Documented!
    /// </summary>
    public partial class Charge
    {
        public ChargeType GetChargeType()
        {
            return (ChargeType)Type;
        }
    }
}
