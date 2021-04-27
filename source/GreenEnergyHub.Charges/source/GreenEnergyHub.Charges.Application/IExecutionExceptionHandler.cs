using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;

namespace GreenEnergyHub.Charges.Application
{
    public interface IExecutionExceptionHandler
    {
        Task ExecuteChargeCommandAsync(
            [NotNull] Func<Task> func,
            [NotNull] ChargeCommand command);
    }
}
