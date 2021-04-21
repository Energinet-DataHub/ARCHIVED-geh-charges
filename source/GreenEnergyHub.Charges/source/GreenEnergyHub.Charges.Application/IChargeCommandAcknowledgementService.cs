﻿using System.Threading.Tasks;
using GreenEnergyHub.Charges.Application.Validation;
using GreenEnergyHub.Queues.ValidationReportDispatcher.Validation;

namespace GreenEnergyHub.Charges.Application
{
    /// <summary>
    /// TODO
    /// </summary>
    public interface IChargeCommandAcknowledgementService
    {
        /// <summary>
        /// Reject the change of charge command.
        /// </summary>
        /// <param name="validationResult"></param>
        /// <returns>TODO 2</returns>
        Task RejectAsync(ChargeCommandValidationResult validationResult);
    }
}
