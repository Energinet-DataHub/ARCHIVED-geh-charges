# Copyright 2020 Energinet DataHub A/S
#
# Licensed under the Apache License, Version 2.0 (the "License2");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
module "func_functionhost" {
  source                                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/function-app?ref=7.0.0"

  name                                      = "functionhost"
  project_name                              = var.domain_name_short
  environment_short                         = var.environment_short
  environment_instance                      = var.environment_instance
  resource_group_name                       = azurerm_resource_group.this.name
  location                                  = azurerm_resource_group.this.location
  vnet_integration_subnet_id                = data.azurerm_key_vault_secret.snet_vnet_integrations_id.value
  private_endpoint_subnet_id                = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  app_service_plan_id                       = data.azurerm_key_vault_secret.plan_shared_id.value
  application_insights_instrumentation_key  = data.azurerm_key_vault_secret.appi_shared_instrumentation_key.value
  log_analytics_workspace_id                = data.azurerm_key_vault_secret.log_shared_id.value
  always_on                                 = true
  health_check_path                         = "/api/monitor/ready"
  health_check_alert_action_group_id        = data.azurerm_key_vault_secret.primary_action_group_id.value
  health_check_alert_enabled                = var.enable_health_check_alerts
  dotnet_framework_version                  = "6"
  use_dotnet_isolated_runtime               = true
  app_settings                              = {
    LOCAL_TIMEZONENAME                                              = "Europe/Copenhagen"
    CURRENCY                                                        = "DKK"
    CHARGE_DB_CONNECTION_STRING                                     = local.MS_CHARGE_DB_CONNECTION_STRING
    DOMAINEVENT_SENDER_CONNECTION_STRING                            = module.sb_charges.primary_connection_strings["send"]
    DOMAINEVENT_MANAGER_CONNECTION_STRING                           = module.sb_charges.primary_connection_strings["manage"]
    DOMAINEVENT_LISTENER_CONNECTION_STRING                          = module.sb_charges.primary_connection_strings["listen"]
    CHARGE_CREATED_TOPIC_NAME                                       = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sbt-charge-created-name)"
    CHARGE_PRICES_UPDATED_TOPIC_NAME                                = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sbt-charge-prices-updated-name)"
    CHARGE_LINKS_ACCEPTED_TOPIC_NAME                                = module.sbt_links_command_accepted.name
    CHARGE_LINKS_REJECTED_TOPIC_NAME                                = module.sbt_links_command_rejected.name
    CHARGE_LINKS_REJECTED_SUBSCRIPTION_NAME                         = "links-command-rejected"
    CHARGE_LINKS_ACCEPTED_SUB_REPLIER                               = "charge-links-accepted-sub-replier"
    CHARGE_LINKS_ACCEPTED_SUB_EVENT_PUBLISHER                       = "charge-links-accepted-sub-event-publisher"
    CHARGE_LINKS_ACCEPTED_SUB_DATA_AVAILABLE_NOTIFIER               = "charge-links-accepted-sub-data-available-notifier"
    CHARGE_LINKS_ACCEPTED_SUB_CONFIRMATION_NOTIFIER                 = "charge-links-accepted-sub-confirmation-notifier"
    CHARGE_LINKS_CREATED_TOPIC_NAME                                 = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sbt-charge-link-created-name)"
    CHARGE_LINKS_RECEIVED_TOPIC_NAME                                = module.sbt_links_command_received.name
    CHARGE_LINKS_RECEIVED_SUBSCRIPTION_NAME                         = "links-command-received-receiver"
    COMMAND_ACCEPTED_TOPIC_NAME                                     = module.sbt_command_accepted.name
    COMMAND_ACCEPTED_RECEIVER_SUBSCRIPTION_NAME                     = "charge-command-accepted-receiver"
    CHARGEACCEPTED_SUB_DATAAVAILABLENOTIFIER                        = "chargeaccepted-sub-dataavailablenotifier"
    COMMAND_ACCEPTED_SUBSCRIPTION_NAME                              = "command-accepted"
    COMMAND_RECEIVED_TOPIC_NAME                                     = module.sbt_command_received.name
    COMMAND_RECEIVED_SUBSCRIPTION_NAME                              = "command-received"
    PRICE_COMMAND_RECEIVED_TOPIC_NAME                               = module.sbt_price_command_received.name
    PRICE_COMMAND_RECEIVED_SUBSCRIPTION_NAME                        = "price-command-received"
    COMMAND_REJECTED_TOPIC_NAME                                     = module.sbt_command_rejected.name
    COMMAND_REJECTED_SUBSCRIPTION_NAME                              = "command-rejected"
    DEFAULT_CHARGE_LINKS_DATA_AVAILABLE_NOTIFIED_TOPIC_NAME         = module.sbt_default_charge_links_available_notified.name
    DEFAULT_CHARGE_LINKS_DATA_AVAILABLE_NOTIFIED_SUBSCRIPTION_NAME  = "default-charge-links-available-notified"
    CREATE_LINKS_REQUEST_QUEUE_NAME                                 = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sbq-create-link-request-name)"
    METERING_POINT_CREATED_TOPIC_NAME                               = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sbt-metering-point-created-name)"
    METERING_POINT_CREATED_SUBSCRIPTION_NAME                        = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sbs-metering-point-created-sub-charges-name)"
    MARKET_PARTICIPANT_CHANGED_TOPIC_NAME                           = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sbt-market-participant-changed-name)"
    MARKET_PARTICIPANT_CHANGED_SUBSCRIPTION_NAME                    = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sbs-market-participant-changed-to-charges-name)"
    CHARGE_PRICE_REJECTED_TOPIC_NAME                                = module.sbt_charge_price_rejected.name
    CHARGE_PRICE_REJECTED_SUBSCRIPTION_NAME                         = "charge-price-rejected"
    
    # Shared resources
    INTEGRATIONEVENT_SENDER_CONNECTION_STRING                       = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sb-domain-relay-send-connection-string)"
    INTEGRATIONEVENT_LISTENER_CONNECTION_STRING                     = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sb-domain-relay-listen-connection-string)"
    INTEGRATIONEVENT_MANAGER_CONNECTION_STRING                      = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sb-domain-relay-manage-connection-string)"

    # Message Hub
    MESSAGEHUB_STORAGE_CONNECTION_STRING                            = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=st-marketres-primary-connection-string)"
    MESSAGEHUB_STORAGE_CONTAINER                                    = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=st-marketres-postofficereply-container-name)"
    MESSAGEHUB_DATAAVAILABLE_QUEUE                                  = "dataavailable"
    MESSAGEHUB_BUNDLEREQUEST_QUEUE                                  = "charges"
    MESSAGEHUB_BUNDLEREPLY_QUEUE                                    = "charges-reply"

    # Shared resources logging
    REQUEST_RESPONSE_LOGGING_CONNECTION_STRING                      = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=st-marketoplogs-primary-connection-string)"
    REQUEST_RESPONSE_LOGGING_CONTAINER_NAME                         = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=st-marketoplogs-container-name)"

    # JWT token
    B2C_TENANT_ID                                                   = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=b2c-tenant-id)"
    BACKEND_SERVICE_APP_ID                                          = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=backend-service-app-id)"
  }

  tags                                      = azurerm_resource_group.this.tags
}