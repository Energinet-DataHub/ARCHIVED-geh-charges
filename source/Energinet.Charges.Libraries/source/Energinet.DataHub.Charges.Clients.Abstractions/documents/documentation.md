# Documentation

The NuGet package [`Energinet.DataHub.Charges.Clients.Abstractions`](https://www.nuget.org/packages/Energinet.DataHub.Charges.Clients.Abstractions/) contains abstractions used by 
Energinet DataHub Charges domain when communicated with clients, e.g. [`Energinet.DataHub.Charges.Clients.Bff`](https://www.nuget.org/packages/Energinet.DataHub.Charges.Clients.Bff/). 

Energinet DataHub is built on the Green Energy Hub platform.

## Getting Started

The library provides

- The charge link model [`ChargeLinkDto`](https://github.com/Energinet-DataHub/geh-charges/blob/main/source/Energinet.Charges.Libraries/source/Energinet.DataHub.Charges.Clients.Abstractions/Models/ChargeLinkDto.cs).
- The type [`IChargeLinksClient`](https://github.com/Energinet-DataHub/geh-charges/blob/main/source/Energinet.Charges.Libraries/source/Energinet.DataHub.Charges.Clients.Abstractions/IChargeLinksClient.cs) to request Charge Links for a Metering Point Id.
