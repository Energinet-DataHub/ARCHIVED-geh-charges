# Documentation

The NuGet package `GreenEnergyHub.Charges.Libraries` provides libraries for interacting with the [Charges domain](https://github.com/Energinet-DataHub/geh-charges).

The library provides
- the type `Energinet.DataHub.Charges.Libraries.DefaultChargeLink.IDefaultChargeLinkClient` to request creation of charge links from defaults and handle the reply
- the type `Energinet.DataHub.Charges.Libraries.DefaultChargeLinkMessages.IDefaultChargeLinkMessagesRequestClient` to request sending notifications to market actors about charge links created from defaults initiated from previous request - and handle the reply
