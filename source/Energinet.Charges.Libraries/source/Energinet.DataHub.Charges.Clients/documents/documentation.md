# Documentation

The NuGet package [`GreenEnergyHub.Charges.Libraries`](https://www.nuget.org/packages/Energinet.DataHub.Charges.Clients/) contains communication API
for clients interacting with the Energinet DataHub charge domain. Energinet DataHub is built on the Green Energy Hub platform.

Learn more about the Green Energy Hub platform at [github.com/Energinet-DataHub/green-energy-hub](https://github.com/Energinet-DataHub/green-energy-hub).

Learn more about the charge domain at [github.com/Energinet-DataHub/geh-charges](https://github.com/Energinet-DataHub/geh-charges).

Learn more about Energinet at [energinet.dk](https://energinet.dk/).

## Getting Started

The library provides

- The type [`IDefaultChargeLinkClient`](https://github.com/Energinet-DataHub/geh-charges/blob/main/source/Energinet.Charges.Libraries/source/Energinet.DataHub.Charges.Clients/DefaultChargeLink/IDefaultChargeLinkClient.cs) to request creation of charge links from defaults and handle the reply
- The type [`IDefaultChargeLinkMessagesClient`](https://github.com/Energinet-DataHub/geh-charges/blob/main/source/Energinet.Charges.Libraries/source/Energinet.DataHub.Charges.Clients/DefaultChargeLinkMessages/IDefaultChargeLinkMessagesClient.cs) to request sending notifications to market actors about charge links created from defaults initiated from previous request - and handle the reply

## Registration
`IDefaultChargeLinkClient` should be registered as a singleton.

`IDefaultChargeLinkMessagesClient` should be used as a singleton.

### Example

            var defaultChargeLinkClientServiceBusRequestSenderProvider =
                new DefaultChargeLinkMessagesClientServiceBusRequestSenderProvider(client, replyToQueueName);
            serviceCollection.AddSingleton<IDefaultChargeLinkMessagesClient>(_ =>
                new DefaultChargeLinkMessagesClient(defaultChargeLinkClientServiceBusRequestSenderProvider));

