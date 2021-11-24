# Documentation

The NuGet package [`Energinet.DataHub.Charges.Clients`](https://www.nuget.org/packages/Energinet.DataHub.Charges.Clients/) contains communication API
for clients interacting with the Energinet DataHub charge domain. Energinet DataHub is built on the Green Energy Hub platform.

Learn more about the Green Energy Hub platform at [github.com/Energinet-DataHub/green-energy-hub](https://github.com/Energinet-DataHub/green-energy-hub).

Learn more about the charge domain at [github.com/Energinet-DataHub/geh-charges](https://github.com/Energinet-DataHub/geh-charges).

Learn more about Energinet at [energinet.dk](https://energinet.dk/).

## Getting Started

The library provides

- The type [`IDefaultChargeLinkClient`](https://github.com/Energinet-DataHub/geh-charges/blob/main/source/Energinet.Charges.Libraries/source/Energinet.DataHub.Charges.Clients/DefaultChargeLink/IDefaultChargeLinkClient.cs) to request creation of charge links from defaults and handle the reply.
- The type [`DefaultChargeLinkReplyReader`](https://github.com/Energinet-DataHub/geh-charges/blob/main/source/Energinet.Charges.Libraries/source/Energinet.DataHub.Charges.Clients/DefaultChargeLink/DefaultChargeLinkReplyReader.cs) to read the respond from the request queue.

## Registration

We have created a independent NuGet package for easier registration when using [SimpleInjector](https://simpleinjector.org/). It is located under the NuGet namespace `Energinet.DataHub.Charges.Clients.SimpleInjector`

`IDefaultChargeLinkClient` should be registered as scoped.

`ServiceBusRequestSenderProvider` should be registered as a singleton.

