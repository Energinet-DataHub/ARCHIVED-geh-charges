using System.Threading.Tasks;
using GreenEnergyHub.Charges.Domain.Events.Local;

namespace GreenEnergyHub.Charges.Application.ChangeOfCharges
{
    /// <summary>
    /// Service for publishing events internally in the domain.
    /// </summary>
    public interface ILocalEventPublisher
    {
        /// <summary>
        /// Publish the local event to the domain.
        /// </summary>
        /// <param name="localEvent"></param>
        /// <returns>No return value.</returns>
        Task PublishAsync(ILocalEvent localEvent);
    }
}
