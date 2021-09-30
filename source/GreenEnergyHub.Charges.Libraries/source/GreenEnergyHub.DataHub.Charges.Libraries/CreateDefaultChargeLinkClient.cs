using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Google.Protobuf;
using GreenEnergyHub.Charges.Commands;

namespace GreenEnergyHub.DataHub.Charges.Clients
{
    public sealed class CreateDefaultChargeLinkClient : IAsyncDisposable
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly string _respondQueName;

        public CreateDefaultChargeLinkClient(string serviceBusConnectionString, string respondQueName)
        {
            _serviceBusClient = new ServiceBusClient(serviceBusConnectionString);
            _respondQueName = respondQueName;
        }

        public async Task CreateDefaultChargeLinksRequestAsync(string meteringPointId)
        {
            if (meteringPointId == null)
                throw new ArgumentNullException(nameof(meteringPointId));

            await using var sender = _serviceBusClient.CreateSender("sbt-create-link-command");
            using var messageBatch = await sender.CreateMessageBatchAsync().ConfigureAwait(false);

            var c = new CreateDefaultChargeLinkMessages { MeteringPointId = meteringPointId };

            var msgBytes = c.ToByteArray();
            if (!messageBatch.TryAddMessage(new ServiceBusMessage(new BinaryData(msgBytes))))
                throw new InvalidOperationException("The message is too large to fit in the batch.");
            await sender.SendMessagesAsync(messageBatch).ConfigureAwait(false);
        }

        public async ValueTask DisposeAsync()
        {
            await _serviceBusClient.DisposeAsync().ConfigureAwait(false);
        }
    }
}
