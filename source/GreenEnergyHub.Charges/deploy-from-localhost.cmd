@echo off

echo =======================================================================================
echo Deploy Azure functions to your private sandbox
echo .
echo Assuming: Domain=charges, Environment=s
echo *** Make sure that you created all local.settings.json settings files ***
echo *** Deployments are executed in separate windows in order to execute in parallel ***
echo =======================================================================================

setlocal

set /p organization=Enter organization used with Terraform (perhaps your initials?): 
set /p doBuild=Build solution ([y]/n)?
rem If you don't know the password, perhaps you can obtain it from the configuration settings of the deployed ChargeCommandReceiver function in Azure portal
set /p sqlPassword=Enter SQL password for 'gehdbadmin' to update db or empty to skip: 
set /p functionHost=Deploy function host ([y]/n)?

IF /I not "%doBuild%" == "n" (
    rem Clean is necessary if e.g. a function project name has changed because otherwise both assemblies will be picked up by deployment
    dotnet clean GreenEnergyHub.Charges.sln -c Release
    dotnet build GreenEnergyHub.Charges.sln -c Release
)

IF not "%sqlPassword%" == "" (
    dotnet run --project source\GreenEnergyHub.Charges.ApplyDBMigrationsApp --configuration Release --^
        "Server=sqlsrv-charges-%organization%-s.database.windows.net;Database=sqldb-charges;Uid=gehdbadmin;Pwd=%sqlPassword%;TrustServerCertificate=true;Persist Security Info=True;"^
        includeSeedData includeTestData
)

IF /I not "%functionHost%" == "n" (
    pushd source\GreenEnergyHub.Charges.FunctionHost\bin\Release\net5.0
    start "Deploy: FunctionHosts" cmd /c "func azure functionapp publish azfun-functionhost-charges-%organization%-s & pause"
    popd
)

endlocal
