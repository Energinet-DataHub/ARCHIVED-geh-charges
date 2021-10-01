using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.MeteringPoints.IntegrationEventContracts;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using GreenEnergyHub.Charges.Commands;
using NodaTime;

namespace GreenEnergyHub.DataHub.Charges.Libraries.DefaultChargeLinkRequest
{
    public sealed class DefaultChargeLinkRequestClient : IAsyncDisposable, IDefaultChargeLinkRequestClient
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

            //var createDefaultChargeLinks = new CreateDefaultChargeLinks { MeteringPointId = meteringPointId };
            var createDefaultChargeLinks = new CreateLinkCommandContract
            {
                MeteringPointId = meteringPointId,
                MeteringPointType = (MeteringPointTypeContract)2,
                StartDateTime = Timestamp.FromDateTime(DateTime.Now.ToUniversalTime()),
            };

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
