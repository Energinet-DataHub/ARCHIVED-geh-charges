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
set /p deployChargeReceiver=Deploy charge receiver ([y]/n)?
set /p functionHost=Deploy function host ([y]/n)?
set /p deployCommandReceiver=Deploy command receiver ([y]/n)?
set /p deployConfirmationSender=Deploy confirmation sender ([y]/n)?
set /p deployRejectionSender=Deploy rejection sender ([y]/n)?

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

rem All (but the last) deployments are opened in separate windows in order to execute in parallel

IF /I not "%deployChargeReceiver%" == "n" (
    pushd source\GreenEnergyHub.Charges.ChargeReceiver\bin\Release\netcoreapp3.1
    start "Deploy: Charge Receiver" cmd /c "func azure functionapp publish azfun-charge-receiver-charges-%organization%-s & pause"
    popd
)

IF /I not "%functionHost%" == "n" (
    pushd source\GreenEnergyHub.Charges.FunctionHost\bin\Release\net5.0
    start "Deploy: FunctionHosts" cmd /c "func azure functionapp publish azfun-functionhost-charges-%organization%-s & pause"
    popd
)

IF /I not "%deployCommandReceiver%" == "n" (
    pushd source\GreenEnergyHub.Charges.ChargeCommandReceiver\bin\Release\netcoreapp3.1
    start "Deploy: Charge Command Receiver" cmd /c "func azure functionapp publish azfun-charge-command-receiver-charges-%organization%-s & pause"
    popd
)

IF /I not "%deployConfirmationSender%" == "n" (
    pushd source\GreenEnergyHub.Charges.ChargeConfirmationSender\bin\Release\netcoreapp3.1
    start "Deploy: Charge Confirmation Sender" cmd /c "func azure functionapp publish azfun-charge-confirmation-sender-charges-%organization%-s & pause"
    popd
)

IF /I not "%deployRejectionSender%" == "n" (
    pushd source\GreenEnergyHub.Charges.ChargeRejectionSender\bin\Release\netcoreapp3.1
	start "Deploy: Charge Rejection Sender" cmd /c "func azure functionapp publish azfun-charge-rejection-sender-charges-%organization%-s & pause"
    popd
)

endlocal
