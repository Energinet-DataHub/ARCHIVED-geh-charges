// See https://aka.ms/new-console-template for more information
using Azlogin;
using Microsoft.Extensions.Configuration;

var envConfiguration = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .Build();

var keyVaultUrl = envConfiguration.GetValue("AZURE_KEYVAULT_URL");

if (keyVaultUrl.StartsWith("https://kv-integrationtest-u-002", StringComparison.InvariantCultureIgnoreCase))
{
    Console.WriteLine("Url matches");
}
else
{
    Console.WriteLine("Url does not match the expected");
}

var keyVaultConfiguration = new ConfigurationBuilder()
    .AddAuthenticatedAzureKeyVault(keyVaultUrl)
    .Build();

Console.WriteLine("Built configuration from keyvault");
