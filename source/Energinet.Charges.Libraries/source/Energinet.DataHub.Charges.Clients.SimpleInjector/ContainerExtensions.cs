using System;
using System.Diagnostics.CodeAnalysis;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Charges.Libraries.DefaultChargeLink;
using Energinet.DataHub.Charges.Libraries.Models;
using Energinet.DataHub.Charges.Libraries.Providers;
using SimpleInjector;

namespace Energinet.DataHub.Charges.Libraries.Clients.SimpleInjector
{
    public static class ContainerExtensions
    {
        /// <summary>
        /// SimpleInjector extension for registering Energinet.DataHub.Charges.Clients NuGet package.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="serviceBusClient">ServiceBusClient required to connected to the shared service bus namespace</param>
        /// <param name="serviceBusRequestSenderConfiguration"></param>
        public static void AddDefaultChargeLinkClient(
            this Container container,
            ServiceBusClient serviceBusClient,
            IServiceBusRequestSenderConfiguration serviceBusRequestSenderConfiguration)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));

            if (serviceBusClient == null)
                throw new ArgumentNullException(nameof(serviceBusClient));

            if (serviceBusRequestSenderConfiguration == null)
                throw new ArgumentNullException(nameof(serviceBusRequestSenderConfiguration));

            container.RegisterSingleton<IServiceBusRequestSenderProvider>(() =>
                new ServiceBusRequestSenderProvider(serviceBusClient, serviceBusRequestSenderConfiguration));

            container.Register<IDefaultChargeLinkClient, DefaultChargeLinkClient>(Lifestyle.Scoped);

            container.Register<IDefaultChargeLinkReplyReader, DefaultChargeLinkReplyReader>(Lifestyle.Scoped);
        }
    }
}
