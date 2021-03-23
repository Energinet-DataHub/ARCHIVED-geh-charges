using NodaTime;

namespace GreenEnergyHub.Charges.Domain.Events
{
    /// <summary>
    /// Contract for events.
    /// </summary>
    public interface IEvent
    {
        /// <summary>
        /// The time the event occurred.
        /// </summary>
        Instant ReceivedTime { get; }

        /// <summary>
        /// An ID that correlates data to an original message.
        /// </summary>
        string CorrelationId { get; }
    }
}
