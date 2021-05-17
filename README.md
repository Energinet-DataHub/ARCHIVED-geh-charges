# Charges

[![codecov](https://codecov.io/gh/Energinet-DataHub/geh-charges/branch/main/graph/badge.svg?token=MGC9QR9S3Q)](https://codecov.io/gh/Energinet-DataHub/geh-charges)

## Intro

Welcome to the charges domain of the [Green Energy Hub project](https://github.com/Energinet-DataHub/green-energy-hub)

The charges domain is responsible for keeping track of price lists of Grid Access Providers and Transmission System Operators (TSO). A charge may be a fee, a subscription or a tariff, where the latter has the option of being a tax.

In addition, the charges domain also keeps track of which of these prices are assigned to a specific metering point at any given moment in time.

These are the business processes maintained by this domain.

| Process  |
| ------------- |
| [Change of Subscription price list](docs/business-processes/change-of-subscription.md) |
| [Change of Fee price list](docs/business-processes/change-of-fee.md) |
| [Change of Tariff price list](docs/business-processes/change-of-tariff.md) |
| [Request for Prices](docs/business-processes/request-for-prices.md) |
| [Settlement master data for metering point - fee, subscription and tariff links](docs/business-processes/settlement_master_data.md)

## Getting Started

Learn how to get started with Green Energy Hub [here](https://github.com/Energinet-DataHub/green-energy-hub/blob/main/docs/getting-started.md).

Learn how to get a development environment up and running for development, debugging, testing or evaluation purposes [here](docs/local-development/README.md).

## Architecture

The architecture diagram below illustrates both current state of the Charges domain as well as future work, last-mentioned being illustrated by grey arrows and components with lowered opacity.

![design](ARCHITECTURE.png)

## Context Streams

TODO: Awaiting drawing from `@MartinFHansen`.

## Domain Road Map

In the current program increment (PI) the planned work is captured by the stated PI goals:

1. Charges are moved to its own Charges domain (and GitHub repository), in order to ensure flexibility, independence of other domains, and stability.
2. The domain can validate, create, update and stop charges.
3. The domain delivers change of charge messages to the [Post Office](https://github.com/Energinet-DataHub/geh-post-office) domain.
4. The domain supports retrieval of historic charges for audit purposes.
5. The domain can be surveilled by a dashboard (one step closer to working software).

## Where can I get more help?

Read about community for Green Energy Hub [here](https://github.com/Energinet-DataHub/green-energy-hub/blob/main/COMMUNITY.md) and learn about how to get involved and get help.

Please note that we have provided a [Dictionary](https://github.com/Energinet-DataHub/green-energy-hub/tree/main/docs/dictionary-and-concepts) to help understand many of the terms used throughout the repository.
