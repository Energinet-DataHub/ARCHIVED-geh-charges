# Infrastructure as code

## Developer Experience

It is possible to deploy infrastructure from `localhost` to a team shared sandbox environment.

Please see prerequisites section at the end of this document.

For installation and learning resources on Terraform refer to [`terraform.io`](https://www.terraform.io).

The steps below solves the problem of otherwise having to provide Terraform variables on each `terraform apply` invocation. All mentioned files are located in the current folder and all commands must likewise be executed from this same folder.

The steps needed are:

- Create file `localhost.tfvars` from `localhost.tfvars.sample` and provide your personal values.
  (The format is the same as other Terraform configuration files)
- Make sure that you're logged in with an account that has access permissions to the selected resource group.
  (Use `az login` from `Azure CLI` to switch account as needed)
- Execute the helper script `deploy-infrastructure-from-localhost.cmd`

Information about the settings:

- `environment_short` is short for environment name and is used in resource name and must be lowercase. Helps ensuring global uniqueness of resource names
- `environment_instance` is an instance number differentiating your team shared sandbox from the main environment and is used in resource name and must be lowercase. Helps ensuring global uniqueness of resource names
- `domain_name_short` is short for domain name and is used in resource name and must be lowercase. Helps ensuring global uniqueness of resource names
- `resource_group_name` the resource group where the infrastructure is provisioned
- `subscription_id` is the service principal object id
- `sharedresources_keyvault_name` is the name of the key vault holding shared/system-wide secrets
- `sharedresources_resource_group_name` is the resource group where shared resources like the shared key vault and more is located
- `notification_email` is your email, used for notifications

If you want to tear down all the resources again simply execute `terraform destroy -auto-approve -var-file="localhost.tfvars"` in the `.working-folder` folder.

**Tip**: If you don't have provisioned any resources yet and encounter problems it might help to delete folder `.terraform` and file `.terraform.lock.hcl` and start over with `terraform init`.

If you still get an error like `Error: Error ensuring Resource Providers are registered: Cannot register provider Microsoft.DevSpaces with Azure Resource Manager: resources.ProvidersClient#Register: Failure responding to request: StatusCode=403` then you need to select the right subscription for your resource group:

```PowerShell
az login
```

search for your subscription and copy the 'id' (a `GUID`) and use it to set subscription:

```PowerShell
az account set --subscription "your subscription id"
```

## Variables in `localhost.tfvars`

This describes what values to use for the various values in the `localhost.tfvars` file.

It is assumed that `Azure CLI` for Windows is installed. If not, guides can be found found later in this file.

### `domain_name_short`

This value should be up to 4 characters.
It differentiates different domains of Green Energy Hub from other domains.

This value will be used as part of the naming scheme for your Azure resources.

### `environment_short`

The value of this need to be 1-3 characters.
The preferred value of `environment_short`is `u`.

This value will be used as part of the naming scheme for your Azure resources.

### `environment_instance`

This value should be up to 5 characters.
In test and production environments it differentiates different instances of an environment, eg. 001, 002 etc. For sandbox environment instance you can include a team name abbreviation, eg. `volt` to make your instance globally unique.

This value will be used as part of the naming scheme for your Azure resources.

#### Warning

A key consisting of `domain_name_short`-`environment_short`-`environment_instance` will be appended to the name of some of your Azure resources as some names need to be unique worldwide.

This means that unless you are targeting a shared environment, you should probably use a 3-5 character word to have the best chance not to clash with others working on the Green Energy Hub. This could for example be your initials.

Please avoid using your organisation name or a single character organisation name it is likely to clash with shared environments, meaning that you could end up blocking one of these.

### `resource_group_name`

This should be set to the name of the resource group you want to deploy into.

### `subscription_id`

This variable has to be filled with the subscription id with access to the resource group you want to deploy to.

So if you are deploying to your own resource group from your own machine, this has to be the subscription ID of the user you use to login to the Azure Portal to see the resource group in question. If deploying to a team shared sandbox you must enter the ID of that subscription.

If the subscription ID is unknown, it can be found using `powershell`.

First, login to the Azure AD with the the account that has access to the resource group.

```PowerShell
az login
```

Then, for personal subscription ID ask the Azure AD for your object ID by using the following command:

```PowerShell
az ad signed-in-user show --query objectId -o tsv
```

The object ID should now be displayed.
This ID is the value you need for `object_id` (hint: it looks like a `GUID`)

Or, for team shared subscription ID ask the Azure AD for your shared subscription ID using this command:

```PowerShell
az account show
```

The subscription ID is just showed as `ID` (hint: it looks like a `GUID`)

### - `sharedresources_keyvault_name`

This is most likely provided by your enterprise architect or cloud architect or similar.

If you're deploying the domain as a development environment - i.e. with not dependencies to other domains - then you can choose this for yourself.

### - `sharedresources_resource_group_name`

This is most likely provided by your enterprise architect or cloud architect or similar.

If you're deploying the domain as a development environment - i.e. with not dependencies to other domains - then you can choose this for yourself.
It's possible to use the same resource group as the resource group used for the domain infrastructure.

### - `notification_email`

The email address that should receive notifications of alerts raised by the infrastructure. Use your own for development.

## Prerequisites

### Installing `Azure CLI`

Follow the [guide](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli).

## Working with Multiple Environments

Sometimes it is necessary to have variations of the infrastructure in different environments. This can be achieved by adding `*_override.tf` files in the respective environment folders in `build/infrastructure/env`.

See more about overriding files with Terraform [here](https://www.terraform.io/docs/language/files/override.html).
