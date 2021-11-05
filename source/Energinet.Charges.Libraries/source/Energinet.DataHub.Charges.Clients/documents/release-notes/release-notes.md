# GreenEnergyHub.Charges.Libraries Release notes

## Version 1.0.8

Protobuf contracts common for both Energinet.Charges.Libraries and elsewhere in the Charges domain are now located in one common place to avoid duplication.

All references to Charges.Clients from elsewhere in Charges is removed.

## Version 1.0.7

Introduced `DefaultChargeLinkClientServiceBusRequestSenderProvider` and `DefaultChargeLinkMessagesClientServiceBusRequestSenderProvider`
which now are used when registering their respective dependent client.

## Version 1.0.6

Fixes links to documentation and release notes on [nuget.org](https://www.nuget.org/packages/Energinet.DataHub.Charges.Clients/).

Improves library documentation.

## Version 1.0.5

Renames `IDefaultChargeLinkMessagesRequestClient` to `IDefaultChargeLinkMessagesClient`.

Updates dependency package versions.

## Version 1.0.0

Initial package release.
