// <copyright file="Charge.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using GreenEnergyHub.Charges.Domain.Charges;

namespace GreenEnergyHub.Charges.QueryApi.ScaffoldedModels
{
    /// <summary>
    /// Documented!
    /// </summary>
    public partial class Charge
    {
        public string GetPublicChargeType()
        {
            return ((ChargeType)ChargeType).ToString();
        }
    }
}
