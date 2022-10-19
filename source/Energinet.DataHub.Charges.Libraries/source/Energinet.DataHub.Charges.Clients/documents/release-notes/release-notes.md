# Energinet.DataHub.Charges.Clients Release notes

## Version 4.1.3

Updated NuGet packages

## Version 4.1.2

Renamed Client contracts namespace from `Energinet.Charges.Contracts` to `Energinet.DataHub.Charges.Contracts`

## Version 4.1.1

Added VatClassification and ChargeDescription to ChargesV1Dto

## Version 4.1.0

Added `GetMarketParticipants` to Charges client.

## Version 4.0.1

Use OIDC for authentication

## Version 4.0.0

Changed search criteria name.
Changed valid from date time offset to allow null in search criteria.
Changed owner id to a list of owner ids in search criteria.

## Version 3.0.17

Added Search to Charges client

## Version 3.0.16

Updated NuGet packages

## Version 3.0.15

Renamed client to Charges

## Version 3.0.14

Removed internal test project reference

## Version 3.0.13

Fix: `ChargeLinksClient` used an incomplete endpoint address after changing to `v1`

## Version 3.0.12

`ChargeLinksClient` calls will now be routed to `v1/ChargeLinks` instead of `v2`

## Version 3.0.11

Updated NuGet packages

## Version 3.0.10

Added custom property `OperationCorrelationId` to `ServiceBusRequestSender`

## Version 3.0.9

Updated NuGet packages

## Version 3.0.8

Bumped because workflow file was updated.

## Version 3.0.7

Removed duplicate NuGet package reference

## Version 3.0.6

Updated NuGet packages

## Version 3.0.5

Updated NuGet packages

## Version 3.0.4

Bumped because workflow file was updated.

## Version 3.0.3

Bumped to latest .Net SDK version 6.0.300 and updated NuGet packages

## Version 3.0.2

Updated NuGet packages

## Version 3.0.1

Updated NuGet packages

## Version 3.0.0

.NET 6 and Azure Function v4 upgrades

## Version 2.0.4

Updated Energinet.DataHub.Core NuGet packages

## Version 2.0.3

Updated NuGet packages

## Version 2.0.2

`IChargeLinksClient` Charges.Clients exposes `ChargeLinkV1Dto`

## Version 2.0.1

`ChargeLinksClientFactory` has been updated to provide a `IChargeLinksClient` with token forwarding

## Version 2.0.0

`IChargeLinksClient` has been updated to use version 2 of the underlying API.
This is a breaking change. Only the UUID charge owner ID of charge owners are now returned in result.
Other attributes of actors must be read from the actor register.

## Version 1.0.25

Updated NuGet packages

## Version 1.0.24

Removed local settings to the unused resources earlier used to request the creation of messages

## Version 1.0.23

Updated NuGet packages

## Version 1.0.22

`ChargeLinkClient` uses `JsonStringEnumConverter` to handle `enum` passed as string

## Version 1.0.21

Revert to using record for `ChargeLinkDto`.

## Version 1.0.20

Use class for `ChargeLinkDto` instead of record.

## Version 1.0.19

`IChargeLinkClient` returns charge links with new naming.

## Version 1.0.18

Use `new JsonSerializerOptions(JsonSerializerDefaults.Web)` in `ChargeLinkClient`.

## Version 1.0.17

Internal restructuring.

## Version 1.0.16

No changes. Build with newer tool chain.

## Version 1.0.15

Added `ChargeLinksClient`

## Version 1.0.14

Updated NuGet dependencies and removed deprecated FxCopAnalyzers

## Version 1.0.13

Removed `IDefaultChargeLinkReplyReader`

## Version 1.0.12

Updated a number of NuGet dependencies.

## Version 1.0.11

Updated the documentation to refer to `Energinet.DataHub.Charges.Clients.SimpleInjector` NuGet package for registration purposes.

Namespaces adjusted to reflect NuGet package id: `Energinet.DataHub.Charges.Clients`

## Version 1.0.10

Integration tests re-enabled in Energinet GitHub pipeline.

## Version 1.0.9

Protobuf contracts common for both Energinet.Charges.Libraries and elsewhere in the Charges domain are now located in one common place to avoid duplication.

All references to Charges.Clients from elsewhere in Charges is removed.

## Version 1.0.8

Missing release notes added

## Version 1.0.7

Introduced `DefaultChargeLinkClientServiceBusRequestSenderProvider` and `DefaultChargeLinkMessagesClientServiceBusRequestSenderProvider`
which now are used when registering their respective dependent client.

## Version 1.0.6

Fixes links to documentation and release notes on [nuget.org](https://www.nuget.org/packages/Energinet.DataHub.Charges.Clients/).

Improves library documentation.

## Version 1.0.5

Renames `IDefaultChargeLinkMessagesRequestClient` to `IDefaultChargeLinkMessagesClient`.

Updates dependency package versions.

## Version 1.0.4

The failed and succeeded methods included in public interface.

## Version 1.0.3

Add functionality to reply when requested to create messages.

## Version 1.0.2

Provide ReplyTo queuename as Service Bus message ApplicationProperty.

## Version 1.0.1

`ReadAsync(byte[] data)` with delegate functionality.

The `ReadAsync(byte[] data)` in conjunction with the `OnSuccess` and `OnFailure` delegates provides the functionality
to deserialize Protobuf data and the flexibility to handle the received data as desired by the receiving domain.

## Version 1.0.0

Initial package release.
