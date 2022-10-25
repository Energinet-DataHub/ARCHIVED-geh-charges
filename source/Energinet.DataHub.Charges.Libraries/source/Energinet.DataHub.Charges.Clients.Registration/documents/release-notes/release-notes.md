# Energinet.DataHub.Charges.Clients.Registrations Release notes

## Version 4.3.1

Added `HasAnyPrices` to `ChargeV1Dto`.

## Version 4.3.0

Added pagination functionality to `SearchChargePricesAsync`.
`SearchChargePricesAsync` now return `ChargePricesV1Dto` instead of a list.

## Version 4.2.1

Added Id to `ChargeV1Dto`.

## Version 4.2.0

Added `SearchChargePricesAsync` to Charges client.
Renamed `SearchCriteriaV1Dto` to `ChargeSearchCriteriaV1Dto`.
Throws exception instead of null if status code is not success.

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

Bumped due to fix in Clients package

## Version 3.0.12

Bumped due to Clients package change

## Version 3.0.11

Updated NuGet packages

## Version 3.0.10

Bumped due to Clients package change

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

## Version 0.1.7

Updated NuGet packages

## Version 0.1.6

Adds `IHttpContextAccessor` injection to enable token forwarding

## Version 0.1.5

Updated NuGet packages

## Version 0.1.4

Updated NuGet packages

## Version 0.1.3

Internal restructuring.

## Version 0.1.2

No changes. Build with newer tool chain.

## Version 0.1.1

Adds an extension to register a `ChargeLinksClient` to an `IServiceCollection`.

## Version 0.1.0

Initial package release containing an extension for [SimpleInjector](https://simpleinjector.org/) to register the `Energinet.DataHub.Charges.Clients` NuGet package.

`ContainerExtensions.AddDefaultChargeLinkClient()` overload added to register with a `Func<ServiceBus>`
