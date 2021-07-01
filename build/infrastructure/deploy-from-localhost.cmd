@echo off
rem Simple helper script for developers to ease deployment of Terraform
rem -----------------------------------------------------------------------------------
rem Set up prerequisites as outlined in README.md and then
rem simply issue the command "deploy-from-localhost" in a command prompt in this folder

rem Merge main and development into new folder

rem Create working folder
if not exist .working-folder mkdir .working-folder
rem Go to a folder that doesn't contain .working-folder because xcopy won't work otherwise
pushd main
del ..\.working-folder\*.tf
xcopy * ..\.working-folder /S /Y /Q >nul
del ..\.working-folder\backend.tf
xcopy ..\env\Development\* ..\.working-folder /S /Y /Q >nul
xcopy ..\localhost.tfvars ..\.working-folder /S /Y /Q >nul
popd

rem Deploy resources of merged setup

pushd .working-folder
terraform init
terraform apply -var-file="localhost.tfvars"
popd
