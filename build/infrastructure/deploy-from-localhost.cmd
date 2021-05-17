rem Simple helper script for developers to ease deployment of Terraform
rem -----------------------------------------------------------------------------------
rem Set up prerequisites as outlined in README.md and then
rem simply issue the command "deploy-from-localhost" in a command prompt in this folder

@echo off

if exist "backend.tf" rename backend.tf backend.tf.tmp

terraform init
terraform plan -var-file="localhost.tfvars"
terraform apply -var-file="localhost.tfvars"

rename backend.tf.tmp backend.tf
