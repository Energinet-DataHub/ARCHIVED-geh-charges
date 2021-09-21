# Infrastructure as code

## Developer Experience

It is possible to deploy infrastructure from `localhost`.

Please see prerequisites section at the end of this document.

For installation and learning resources on Terraform refer to [`terraform.io`](https://www.terraform.io).

The steps below solves the problem of otherwise having to provide Terraform variables on each `terraform apply` invocation. All mentioned files are
located in the current folder and all commands must likewise be executed from this same folder.

The steps needed are:

- Create file `localhost.tfvars` from `localhost.tfvars.sample` and provide your personal values.
  (The format is the same as other Terraform configuration files)
- Make sure that you're logged in with an account that has access permissions to the selected resource group.
  (Use `az login` from `Azure CLI` to switch account as needed)
- Execute the helper script `deploy-from-localhost.cmd`

Information about the settings:

- `environment` is short for environment name and is used in resource name and must be lowercase. Helps ensuring global uniqueness of resource names
- `organisation` is your organisation name and is used in resource name and must be lowercase. Helps ensuring global uniqueness of resource names
- `resource_group_name` the resource group where the infrastructure is provisioned
- `project` is the project or domain name and is used in resource name and must be lowercase. Helps ensuring global uniqueness of resource names
- `tenant_id` is the cloud service provider tenant id
- `spn_object_id` is the service principal object id
- `sharedresources_keyvault_name` is the name of the key vault holding shared/system-wide secrets
- `sharedresources_resource_group_name` is the resource group where shared resources like the shared key vault and more is located

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

### `environment`

The value of this need to be 1-3 characters.

#### Warning

The environment value will be appended to the name of some of your Azure resources as some names need to be unique worldwide.

This means that unless you are targeting a shared environment, you should probably use a 3 character word to have the best chance not to clash with others working on the Green Energy Hub. This could for example be your initials.

Please avoid using single character environment names as these are likely to clash with shared environments, meaning that you could end up blocking one of these.

### `organisation`

This value should be 4 characters and describe the organisation you represent.

For Energinet, this should be set to `endk`

This value will also be used as part of the naming scheme for your Azure resources.

### `resource_group_name`

This should be set to the name of the resource group you want to deploy into.

### `project`

This should be set to the name of the project this deployment is part of, usually the same as the domain name.

For use with this domain, the recommended value is `charges`, but it really is up to developer.

Make sure it does not get any longer than 7 characters.

### `tenant_id`

The value to use can be found by running the following in `powershell` and supply login for the user that has access to the resource group:

```PowerShell
az login
```

In the result, a field will be called `tenantId`.

The value is what is needed for `tenantId` (hint: it looks like a `GUID`)

### `spn_object_id`

This variable has to be filled with the object id of the user that has access to the resource group you want to deploy to.

So if you are deploying to your own resource group from your own machine, this has to be the object id of the user you use to login to the Azure Portal to see the resource group in question.

If the Object ID is unknown, it can be found with using `powershell`.

First, login to the Azure AD with the the account that has access to the resource group.

```PowerShell
az login
```

Then, ask the Azure AD for your object ID by using the following command:

```PowerShell
az ad signed-in-user show --query objectId -o tsv
```

The object ID should now be displayed.

This ID is the value you need for `object_id` (hint: it looks like a `GUID`)

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
