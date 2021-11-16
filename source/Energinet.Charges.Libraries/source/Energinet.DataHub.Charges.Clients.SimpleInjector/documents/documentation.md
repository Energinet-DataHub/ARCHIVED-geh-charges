# Documentation

The NuGet package [`GreenEnergyHub.Charges.Libraries.Clients.SimpleInjector`](https://www.nuget.org/packages/Energinet.DataHub.Charges.Clients.SimpleInjector/) contains functionality to register the GreenEnergyHub.Charges.Libraries.Clients NuGet package.

Learn more about the Green Energy Hub platform at [github.com/Energinet-DataHub/green-energy-hub](https://github.com/Energinet-DataHub/green-energy-hub).

Learn more about the charge domain at [github.com/Energinet-DataHub/geh-charges](https://github.com/Energinet-DataHub/geh-charges).

Learn more about Energinet at [energinet.dk](https://energinet.dk/).

## Getting Started

The library provides

- The type [`ContainerExtensions`](https://github.com/Energinet-DataHub/geh-charges/blob/main/source/Energinet.Charges.Libraries/source/Energinet.DataHub.Charges.Clients.SimpleInjector/ContainerExtensons.cs) to extend the SimpleInjector `Container`

Which adds the extension method `AddDefaultChargeLinkClient` to register the GreenEnergyHub.Charges.Libraries.Clients NuGet package.
