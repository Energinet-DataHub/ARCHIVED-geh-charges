# Energinet.DataHub.Charges.Clients.Registrations Release notes

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
