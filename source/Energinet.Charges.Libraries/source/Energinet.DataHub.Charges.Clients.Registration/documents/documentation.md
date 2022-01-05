# Documentation

The NuGet package `Energinet.DataHub.Charges.Clients.Registration` contains functionality to register the Energinet.DataHub.Charges.Clients NuGet package using SimpleInjector.

The NuGet package also contains functionality to register the `ChargeLinksClient` to an `IServiceCollection`.

To learn more about the `Energinet.DataHub.Charges.Clients` NuGet package or GreenEnergyHub in general, please visit this [`link`](https://www.nuget.org/packages/Energinet.DataHub.Charges.Clients/)

## Getting Started

The library provides

- The type [`ContainerExtensions`](https://github.com/Energinet-DataHub/geh-charges/blob/main/source/Energinet.Charges.Libraries/source/Energinet.DataHub.Charges.Clients.Registration/DefaultChargeLink/SimpleInjector/ContainerExtensions.cs) to extend the SimpleInjector `Container`

Which adds the extension method `AddDefaultChargeLinkClient` to register the Energinet.DataHub.Charges.Clients NuGet package.

- The type [`ServiceCollectionExtensions`](https://github.com/Energinet-DataHub/geh-charges/blob/main/source/Energinet.Charges.Libraries/source/Energinet.DataHub.Charges.Clients.Registration/ChargeLinks/ServiceCollectionExtensions/ServiceCollectionExtensions.cs)

Provides an extension method for registering the `ChargeLinksClient`.
