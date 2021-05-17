@echo off

echo Deploy Azure functions to your private sandbox
echo Assuming: Domain=charges, Environment=s
echo *** Make sure that you created all local.settings.json settings files ***

set /p project=Enter project used with Terraform (perhaps your initials?)
set /p doBuild=Build solution ([y]/n)?
set /p deployMessageReceiver=Deploy message receiver ([y]/n)?
set /p deployCommandReceiver=Deploy command receiver ([y]/n)?
set /p deployAckSender=Deploy positive acknowledgement sender ([y]/n)?
set /p deployNegAckSender=Deploy negative acknowledgement sender ([y]/n)?

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

IF /I not "%deployAckSender%" == "n" (
    pushd source\GreenEnergyHub.Charges.ChargeAcknowledgementSender\bin\Release\netcoreapp3.1
    func azure functionapp publish azfun-charge-acknowledgement-sender-charges-%project%-s
    popd
)

IF /I not "%deployNegAckSender%" == "n" (
    pushd source\GreenEnergyHub.Charges.ChargeNegativeAcknowledgementSender\bin\Release\netcoreapp3.1
    func azure functionapp publish azfun-charge-negative-acknowledgement-sender-charges-%project%-s
	popd
)
