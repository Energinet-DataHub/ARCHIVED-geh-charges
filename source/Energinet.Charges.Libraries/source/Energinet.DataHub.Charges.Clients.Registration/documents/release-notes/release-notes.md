# Energinet.DataHub.Charges.Clients.Registrations Release notes

## Version 0.1.1

Adds an extension to register a `ChargeLinkClient` to an `IServiceCollection`.

## Version 0.1.0

Initial package release containing an extension for [SimpleInjector](https://simpleinjector.org/) to register the `Energinet.DataHub.Charges.Clients` NuGet package.

`ContainerExtensions.AddDefaultChargeLinkClient()` overload added to register with a `Func<ServiceBus>`
