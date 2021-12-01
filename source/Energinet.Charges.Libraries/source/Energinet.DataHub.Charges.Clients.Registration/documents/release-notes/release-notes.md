# Energinet.DataHub.Charges.Clients.Registrations Release notes

## Version 0.1.0

Initial package release containing an extension for [SimpleInjector](https://simpleinjector.org/) to register the `Energinet.DataHub.Charges.Clients` NuGet package.

`ContainerExtensions.AddDefaultChargeLinkClient()` overload added too register with a `Func<ServiceBus>`
