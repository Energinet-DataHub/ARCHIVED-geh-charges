@echo off

echo =======================================================================================
echo Execute start or stop command of Azure functions in your private sandbox
echo .
echo Assuming: Domain=charges, Environment=s
echo =======================================================================================

setlocal

set /p organization=Enter organization used with Terraform (perhaps your initials?):
set /p resourceGroup=Enter resource group:
set /p command=Write start or stop command:
set /p chargeReceiver=Execute for charge receiver ([y]/n)?
set /p chargeLinkReceiver=Execute for charge link receiver ([y]/n)?
set /p chargeLinkEventPublisher=Execute for charge link event publisher ([y]/n)?
set /p commandReceiver=Execute for command receiver ([y]/n)?
set /p confirmationSender=Execute for confirmation sender ([y]/n)?
set /p rejectionSender=Execute for rejection sender ([y]/n)?
set /p meteringPointCreatedReceiver=Execute for metering point created receiver ([y]/n)?
set /p chargeLinkCommandReceiver=Execute for charge link command receiver([y]/n)?
set /p createLinkCommandReceiver=Execute for create link command receiver ([y]/n)?

rem All (but the last) command are opened in separate windows in order to execute in parallel

IF /I not "%chargeReceiver%" == "n" (
    pushd source\GreenEnergyHub.Charges.ChargeReceiver\bin\Release\netcoreapp3.1
    start "Execute command for: Charge Receiver" cmd /c "az functionapp %command% --name azfun-charge-receiver-charges-%organization%-s --resource-group %resourceGroup% & pause"
    popd
)

IF /I not "%chargeLinkReceiver%" == "n" (
    pushd source\GreenEnergyHub.Charges.ChargeLinkReceiver\bin\Release\net5.0
    start "Execute command for: Charge Link Receiver" cmd /c "az functionapp %command% --name azfun-link-receiver-charges-%organization%-s --resource-group %resourceGroup% & pause"
    popd
)

IF /I not "%chargeLinkEventPublisher%" == "n" (
    pushd source\GreenEnergyHub.Charges.ChargeLinkEventPublisher\bin\Release\net5.0
    start "Execute command for: Charge Link Event Publisher" cmd /c "az functionapp %command% --name azfun-link-event-publisher-charges-%organization%-s --resource-group %resourceGroup% & pause"
    popd
)

IF /I not "%commandReceiver%" == "n" (
    pushd source\GreenEnergyHub.Charges.ChargeCommandReceiver\bin\Release\netcoreapp3.1
    start "Execute command for: Charge Command Receiver" cmd /c "az functionapp %command% --name azfun-charge-command-receiver-charges-%organization%-s --resource-group %resourceGroup% & pause"
    popd
)

IF /I not "%rejectionSender%" == "n" (
    pushd source\GreenEnergyHub.Charges.ChargeConfirmationSender\bin\Release\netcoreapp3.1
    start "Execute command for: Charge Confirmation Sender" cmd /c "az functionapp %command% --name azfun-charge-confirmation-sender-charges-%organization%-s --resource-group %resourceGroup% & pause"
    popd
)

IF /I not "%meteringPointCreatedReceiver%" == "n" (
    pushd source\GreenEnergyHub.Charges.ChargeRejectionSender\bin\Release\netcoreapp3.1
	start "Execute command for: Charge Rejection Sender" cmd /c "az functionapp %command% --name azfun-charge-rejection-sender-charges-%organization%-s --resource-group %resourceGroup% & pause"
    popd
)

IF /I not "%chargeLinkCommandReceiver%" == "n" (
    pushd source\GreenEnergyHub.Charges.MeteringPointCreatedReceiver\bin\Release\netcoreapp3.1
    start "Execute command for: Metering Point Created Receiver" cmd /c "az functionapp %command% --name azfun-metering-point-created-receiver-charges-%organization%-s --resource-group %resourceGroup% & pause"
    popd
)

IF /I not "%chargeLinkCommandReceiver%" == "n" (
    pushd source\GreenEnergyHub.Charges.ChargeLinkCommandReceiver\bin\Release\net5.0
    start "Execute command for: Charge Link Command Receiver" cmd /c "az functionapp %command% --name azfun-link-command-receiver-charges-%organization%-s --resource-group %resourceGroup% & pause"
    popd
)

IF /I not "%createLinkCommandReceiver%" == "n" (
    pushd source\GreenEnergyHub.Charges.CreateLinkCommandReceiver\bin\Release\net5.0
    start "Execute command for: Create Link Command Receiver" cmd /c "az functionapp %command% --name azfun-create-link-command-receiver-charges-%organization%-s --resource-group %resourceGroup% & pause"
    popd
)

endlocal
