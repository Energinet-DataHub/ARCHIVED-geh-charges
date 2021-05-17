@echo off

echo Deploy Azure functions to your private sandbox
echo Assuming: Domain=charges, Environment=s
echo *** Make sure that you created all local.settings.json settings files ***

setlocal

set /p project=Enter project used with Terraform (perhaps your initials?): 
set /p doBuild=Build solution ([y]/n)?
set /p deployMessageReceiver=Deploy message receiver ([y]/n)?
set /p deployCommandReceiver=Deploy command receiver ([y]/n)?
set /p deployConfirmationSender=Deploy confirmation sender ([y]/n)?
set /p deployRejectionSender=Deploy rejection sender ([y]/n)?

IF /I not "%doBuild%" == "n" (
    dotnet build GreenEnergyHub.Charges.sln -c Release
)

IF /I not "%deployMessageReceiver%" == "n" (
    pushd source\GreenEnergyHub.Charges.MessageReceiver\bin\Release\netcoreapp3.1
    func azure functionapp publish azfun-message-receiver-charges-%project%-s
    popd
)

IF /I not "%deployCommandReceiver%" == "n" (
    pushd source\GreenEnergyHub.Charges.ChargeCommandReceiver\bin\Release\netcoreapp3.1
    func azure functionapp publish azfun-charge-command-receiver-charges-%project%-s
    popd
)

IF /I not "%deployConfirmationSender%" == "n" (
    pushd source\GreenEnergyHub.Charges.ChargeConfirmationSender\bin\Release\netcoreapp3.1
    func azure functionapp publish azfun-charge-confirmation-sender-charges-%project%-s
    popd
)

IF /I not "%deployRejectionSender%" == "n" (
    pushd source\GreenEnergyHub.Charges.ChargeRejectionSender\bin\Release\netcoreapp3.1
    func azure functionapp publish azfun-charge-rejection-sender-charges-%project%-s
	popd
)

endlocal
