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
- Temporarily rename `backend.tf` to `backend.tf.exclude`.
  (This is needed to avoid Terraform from picking it up because the settings in the file are not overwritten by your
  `localhost.tfvars` file. Also Terraform currently doesn't support explicit exclusion. See [this issue](https://github.com/hashicorp/terraform/issues/2253))
- Execute `terraform init`
- Execute `terraform apply -var-file="localhost.tfvars"`
- Restore file name of `backend.tf`

Information about the settings:

- `environment` is used in resource name and must be lowercase
- `organisation` is used in resource name and must be lowercase

If you want to tear down all the resources again simply execute `terraform destroy -auto-approve`.

**Tip**: If you don't have provisioned any resources yet and encounter problems it might help to delete folder `.terraform` and file `.terraform.lock.hcl` and start over with `terraform init`.

## Variables in `localhost.tfvars`

This describes what values to use for the various values in the `localhost.tfvars` file.

It is assumed that `Azure CLI` for Windows and the Azure AD module for `powershell` is installed. If not, guides can be found found later in this file.

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

First, login to the Azure AD with the the account that has access to the resource group. Run the following command and supply your login information:

```PowerShell
Connect-AzureAd
```

Then, ask the Azure AD for your object ID by using the following command:

```PowerShell
Get-AzureADUser -searchstring "username@yourdomain.com"
```

The object ID should now be displayed as the first column in the result.

This ID is the value you need for `object_id` (hint: it looks like a `GUID`)

# Prerequisites

## Installing `Azure CLI`

Follow the [guide](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli).

## Installing `Azure AD for powershell`

Run your `powershell` as an administrator.

The following command installs the module:

```PowerShell
Install-Module -Name AzureAD
```
