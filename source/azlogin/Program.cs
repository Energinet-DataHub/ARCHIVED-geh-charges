// See https://aka.ms/new-console-template for more information
using Azlogin;
using Microsoft.Extensions.Configuration;

var envConfiguration = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .Build();

var keyVaultUrl = envConfiguration.GetValue("AZURE_KEYVAULT_URL");

var keyVaultConfiguration = new ConfigurationBuilder()
    .AddAuthenticatedAzureKeyVault(keyVaultUrl)
    .Build();

Console.WriteLine("Built configuration from keyvault");
