# GreenEnergyHub.Charges.Libraries Release notes

## Version 1.0.6

Fixes links to documentation and release notes on [nuget.org](https://www.nuget.org/packages/Energinet.DataHub.Charges.Clients/).

Improves library documentation.
## Version 1.0.5

Renames `IDefaultChargeLinkMessagesRequestClient` to `IDefaultChargeLinkMessagesClient`.

Updates dependency package versions.

## Version 1.0.4

Adds functionality to reply when requested to create messages.

## Version 1.0.3

Provide ReplyTo queuename as Service Bus message ApplicationProperty.

## Version 1.0.2

Provide ReplyTo queuename as Service Bus message ApplicationProperty.

## Version 1.0.1

`ReadAsync(byte[] data)` with delegate functionality.

The `ReadAsync(byte[] data)` in conjunction with the `OnSuccess` and `OnFailure` delegates provides the functionality
to deserialize Protobuf data and the flexibility to handle the received data as desired by the receiving domain.

## Version 1.0.0

Initial package release.
