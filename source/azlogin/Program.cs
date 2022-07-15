// See https://aka.ms/new-console-template for more information
using Azlogin;
using Microsoft.Extensions.Configuration;

var envConfiguration = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .Build();

var keyVaultUrl = envConfiguration.GetValue("AZURE_KEYVAULT_URL");
var shouldExcludeManagedIdentityCredential = envConfiguration.GetValue("ExcludeManagedIdentityCredential");

if (!bool.TryParse(shouldExcludeManagedIdentityCredential, out var excludeManagedIdentityCredential))
    excludeManagedIdentityCredential = false;

var keyVaultConfiguration = new ConfigurationBuilder()
    .AddAuthenticatedAzureKeyVault(keyVaultUrl, excludeManagedIdentityCredential)
    .Build();

Console.WriteLine("Built configuration from keyvault");
