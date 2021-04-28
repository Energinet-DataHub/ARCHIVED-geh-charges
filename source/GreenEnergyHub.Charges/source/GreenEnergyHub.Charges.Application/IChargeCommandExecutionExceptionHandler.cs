using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;

namespace GreenEnergyHub.Charges.Application
{
    public interface IChargeCommandExecutionExceptionHandler
    {
        Task<TResult> ExecuteChargeCommandAsync<TResult>(
            [NotNull] Func<Task<TResult>> func,
            [NotNull] ChargeCommand command);

        Task ExecuteChargeCommandAsync(
            [NotNull] Func<Task> func,
            [NotNull] ChargeCommand command);
    }
}
