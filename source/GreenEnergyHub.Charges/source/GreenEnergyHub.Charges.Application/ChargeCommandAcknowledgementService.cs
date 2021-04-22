using System.Threading.Tasks;
using GreenEnergyHub.Charges.Application.ChangeOfCharges;
using GreenEnergyHub.Charges.Application.Validation;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;

namespace GreenEnergyHub.Charges.Application
{
    public class ChargeCommandAcknowledgementService : IChargeCommandAcknowledgementService
    {
        private readonly IInternalEventPublisher _internalEventPublisher;
        private readonly IChargeCommandRejectedEventFactory _chargeCommandRejectedEventFactory;
        private readonly IChargeCommandAcceptedEventFactory _chargeCommandAcceptedEventFactory;

        public ChargeCommandAcknowledgementService(IInternalEventPublisher internalEventPublisher, IChargeCommandRejectedEventFactory chargeCommandRejectedEventFactory, IChargeCommandAcceptedEventFactory chargeCommandAcceptedEventFactory)
        {
            _internalEventPublisher = internalEventPublisher;
            _chargeCommandRejectedEventFactory = chargeCommandRejectedEventFactory;
            _chargeCommandAcceptedEventFactory = chargeCommandAcceptedEventFactory;
        }

        public async Task RejectAsync(ChargeCommandValidationResult validationResult)
        {
            var chargeEvent = _chargeCommandRejectedEventFactory.CreateEvent(validationResult);
            await _internalEventPublisher.PublishAsync(chargeEvent).ConfigureAwait(false);
        }

        public async Task AcceptAsync(ChargeCommand command)
        {
            var chargeEvent = _chargeCommandAcceptedEventFactory.CreateEvent(command);
            await _internalEventPublisher.PublishAsync(chargeEvent).ConfigureAwait(false);
        }
    }
}
