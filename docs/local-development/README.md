# Getting Started as a Developer #

These are the basic steps to set up your own environment for development.

1. Set up infrastructure in Azure using Terraform
2. Create database
3. Publish Azure Functions
4. Test by executing a business process

FAQ

1. How do I debug my function?
2. Tips for trouble shooting
3. How do I use a local SQL Server when doing db development?

## Set up infrastructure in Azure using Terraform ##

Instructions are [here](../../build/infrastructure/README.md).

There is a little utility script that can be used for subsequent Terraform deployments [here](../../build/infrastructure/deploy-from-localhost.cmd).

## Create database ##

Build and execute the command line tool `ApplyDBMigrationsApp` with connection string to your deployed database. The tool is in [this solution](../../source/GreenEnergyHub.Charges/GreenEnergyHub.Charges.sln). Detailed instructions are [here](../../source/GreenEnergyHub.Charges/source/GreenEnergyHub.Charges.ApplyDBMigrationsApp/README.md).

The connection string can be found in the configuration of the charge command receiver function in the Azure portal.

TODO: `includeSeedData` and `includeTestData`

## Publish Azure Functions ##

Instructions are [here](publish-function-azure-sandbox.md).

## Test by executing a business process ##

TODO

* E.g. Postman
* Example messages
    * Requires SQL seed and test data

## FAQ ##

### How do I debug my function? ###

TODO

* `local.settings.sample.json`

### Tips for trouble shooting ###

TODO

* inspect topics using Azure Service Bus Explorer
* inspect database (how to obtain credentials)

### How do I use a local SQL Server when doing db development? ###

TODO
