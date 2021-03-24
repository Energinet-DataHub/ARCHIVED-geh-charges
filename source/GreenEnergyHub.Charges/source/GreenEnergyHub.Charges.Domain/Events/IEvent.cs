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
        Instant PublishedTime { get; }

        /// <summary>
        /// An ID that correlates data to an original message.
        /// </summary>
        string CorrelationId { get; }
    }
}
