@echo off

echo ========================================================================================
echo Deploy Azure functions to your team shared sandbox
echo .
echo *** Make sure that you created all local.settings.json settings files ***
echo *** Bad request error while syncing function triggers doesn't seem to cause problems ***
echo ========================================================================================

setlocal

set /p domain_name_short=Enter domain name abbreviation used with Terraform (perhaps chgs?):
set /p environment_name_short=Enter environment name abbreviation used with Terraform (perhaps u?):
set /p environment_instance=Enter environment instance used with Terraform (perhaps 2volt?):
set /p doBuild=Build solution ([y]/n)?
rem If you don't know the password, perhaps you can obtain it from the configuration settings of the deployed ChargeCommandReceiver function in Azure portal
set /p sqlPassword=Enter SQL password for 'gehdbadmin' to update db or empty to skip:
set /p shared_environment_instance=Enter environment instance for shared resources (perhaps 002?):
set /p functionHost=Deploy function host ([y]/n)?

IF not "%sqlPassword%" == "" (
    dotnet run --project source\GreenEnergyHub.Charges.ApplyDBMigrationsApp --configuration Release --^
        "Server=sql-data-sharedres-%environment_name_short%-%shared_environment_instance%.database.windows.net;Database=sqldb-data-%domain_name_short%-%environment_name_short%-%environment_instance%;Uid=gehdbadmin;Pwd=%sqlPassword%;TrustServerCertificate=true;Persist Security Info=True;"^
        includeSeedData includeTestData
)

IF /I not "%doBuild%" == "n" (
    rem Clean is necessary if e.g. a function project name has changed because otherwise both assemblies will be picked up by deployment
    dotnet clean GreenEnergyHub.Charges.sln -c Release
    dotnet build GreenEnergyHub.Charges.sln -c Release
)

IF /I not "%functionHost%" == "n" (
    pushd source\GreenEnergyHub.Charges.FunctionHost\bin\Release\net5.0
    func azure functionapp publish func-functionhost-%domain_name_short%-%environment_name_short%-%environment_instance% & pause
    popd
)

endlocal
