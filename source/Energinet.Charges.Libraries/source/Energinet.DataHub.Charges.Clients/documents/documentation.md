# Documentation

The NuGet package [`GreenEnergyHub.Charges.Libraries`](https://www.nuget.org/packages/Energinet.DataHub.Charges.Clients/) contains communication API
for clients interacting with the Energinet DataHub charge domain. Energinet DataHub is built on the Green Energy Hub platform.

Learn more about the Green Energy Hub platform at https://github.com/Energinet-DataHub/green-energy-hub.

Learn more about the charge domain at https://github.com/Energinet-DataHub/geh-charges.

Learn more about Energinet at https://energinet.dk/.

## Getting Started

The library provides

- The type `Energinet.DataHub.Charges.Libraries.DefaultChargeLink.IDefaultChargeLinkClient` to request creation of charge links from defaults and handle the reply
- The type `Energinet.DataHub.Charges.Libraries.DefaultChargeLinkMessages.IDefaultChargeLinkMessagesRequestClient` to request sending notifications to market actors about charge links created from defaults initiated from previous request - and handle the reply
