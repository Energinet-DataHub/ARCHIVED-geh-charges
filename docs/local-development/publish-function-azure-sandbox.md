# Publishing Azure functions

As a developer, you can publish functions to Azure using Azure Functions Core Tools.

Please see prerequisites section at the end of this document.

Following Terraform deploy to an Azure Resource Group you can publish functions:

Azure login:

```PowerShell
az login
```

Publish Azure function:

```PowerShell
func azure functionapp publish {Azure function name}
```

Azure function naming: `azfun-[name]-[project]-[organisation]-[environment]`, eg. `azfun-message-receiver-charges-endk-[environment]`

## Prerequisites

Azure Functions Core Tools: [`Azure Functions Core Tools download`](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local?tabs=windows%2Ccsharp%2Cbash#v2)

Install `v3.x - Windows 64-bit`.
