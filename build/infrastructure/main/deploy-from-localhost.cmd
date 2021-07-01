@echo off
rem Simple helper script for developers to ease deployment of Terraform
rem -----------------------------------------------------------------------------------
rem Set up prerequisites as outlined in README.md and then
rem simply issue the command "deploy-from-localhost" in a command prompt in this folder

rem Merge main and development into new folder

if not exist ..\.development-merged mkdir ..\.development-merged
del ..\.development-merged\*.tf
copy *.tf ..\.development-merged /Y >nul
del ..\.development-merged\backend.tf
copy *.tfvars ..\.development-merged /Y >nul
copy *.json ..\.development-merged /Y >nul
if not exist ..\.development-merged\modules mkdir ..\.development-merged\modules
xcopy modules\* ..\.development-merged\modules /S /Y /Q >nul
xcopy ..\env\Development\* ..\.development-merged /S /Y /Q >nul

rem Deploy resources of merged setup

pushd ..\.development-merged
terraform init
terraform plan -var-file="localhost.tfvars" -out=tfplan
terraform apply -var-file="localhost.tfvars" tfplan
popd
