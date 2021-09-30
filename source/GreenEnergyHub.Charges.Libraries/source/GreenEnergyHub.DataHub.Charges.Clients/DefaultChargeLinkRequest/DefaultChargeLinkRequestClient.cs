using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Google.Protobuf;
using GreenEnergyHub.Charges.Commands;

namespace GreenEnergyHub.DataHub.Charges.Libraries.DefaultChargeLinkRequest
{
    public sealed class DefaultChargeLinkRequestClient : IAsyncDisposable
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly string _respondQueue;

        public DefaultChargeLinkRequestClient(string serviceBusConnectionString, string respondQueue)
        {
            _serviceBusClient = new ServiceBusClient(serviceBusConnectionString);
            _respondQueue = respondQueue;
        }

        public async Task CreateDefaultChargeLinksRequestAsync(
            string meteringPointId,
            string correlationId)
        {
            if (meteringPointId == null)
                throw new ArgumentNullException(nameof(meteringPointId));

            await using var sender = _serviceBusClient.CreateSender("sbt-create-link-command");

            var createDefaultChargeLinks = new CreateDefaultChargeLinks { MeteringPointId = meteringPointId };

            await sender.SendMessageAsync(new ServiceBusMessage
            {
                Body = new BinaryData(createDefaultChargeLinks.ToByteArray()),
                ReplyTo = _respondQueue,
                CorrelationId = correlationId,
            }).ConfigureAwait(false);
        }

        public async ValueTask DisposeAsync()
        {
            await _serviceBusClient.DisposeAsync().ConfigureAwait(false);
        }
    }
}
