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
    DOMAINEVENT_SENDER_CONNECTION_STRING                            = data.azurerm_key_vault_secret.sb_domain_relay_send_connection_string.value
    DOMAINEVENT_MANAGER_CONNECTION_STRING                           = data.azurerm_key_vault_secret.sb_domain_relay_manage_connection_string.value
    DOMAINEVENT_LISTENER_CONNECTION_STRING                          = data.azurerm_key_vault_secret.sb_domain_relay_listen_connection_string.value
    
    # Topics
    DOMAIN_EVENTS_TOPIC_NAME                                        = "sbt-charges-domain-events"
    CHARGE_CREATED_TOPIC_NAME                                       = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sbt-charge-created-name)"
    CHARGE_PRICES_UPDATED_TOPIC_NAME                                = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sbt-charge-prices-updated-name)"
    CHARGE_LINKS_CREATED_TOPIC_NAME                                 = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sbt-charge-link-created-name)"

    # Charge domain event subscriptions
    CHARGE_LINKS_COMMAND_REJECTED_SUBSCRIPTION_NAME                 = "sbtsub-charges-charge-links-command-rejected"
    CHARGE_LINKS_ACCEPTED_PUBLISH_SUBSCRIPTION_NAME                 = "sbtsub-charges-charge-links-accepted-publish"
    CHARGE_LINKS_ACCEPTED_DATAAVAILABLE_SUBSCRIPTION_NAME           = "sbtsub-charges-charge-links-accepted-dataavailable"
    CHARGE_LINKS_ACCEPTED_CONFIRMATION_SUBSCRIPTION_NAME            = "sbtsub-charges-charge-links-accepted-confirmation"    
    CHARGE_LINKS_COMMAND_RECEIVED_SUBSCRIPTION_NAME                 = "sbtsub-charges-charge-links-command-received"
    CHARGE_COMMAND_ACCEPTED_PUBLISH_SUBSCRIPTION_NAME               = "sbtsub-charges-charge-command-accepted-publish"
    CHARGE_ACCEPTED_DATAAVAILABLE_SUBSCRIPTION_NAME                 = "sbtsub-charges-charge-accepted-dataavailable"
    CHARGE_COMMAND_ACCEPTED_SUBSCRIPTION_NAME                       = "sbtsub-charges-charge-command-accepted"
    CHARGE_COMMAND_RECEIVED_SUBSCRIPTION_NAME                       = "sbtsub-charges-charge-command-received"
    CHARGE_COMMAND_REJECTED_SUBSCRIPTION_NAME                       = "sbtsub-charges-charge-command-rejected"
    CHARGE_PRICE_COMMAND_RECEIVED_SUBSCRIPTION_NAME                 = "sbtsub-charges-charge-price-command-received"
    CHARGE_PRICE_REJECTED_SUBSCRIPTION_NAME                         = "sbtsub-charges-charge-price-rejected"
    CHARGE_PRICE_CONFIRMED_SUBSCRIPTION_NAME                        = "sbtsub-charges-charge-price-confirmed"
    CHARGE_PRICE_CONFIRMED_DATAAVAILABLE_SUBSCRIPTION_NAME          = "sbtsub-charges-charge-price-confirmed-dataavail"
    CHARGE_PRICE_CONFIRMED_PUBLISH_SUBSCRIPTION_NAME                = "sbtsub-charges-charge-price-confirmed-publish"
    DEFAULT_CHARGE_LINKS_DATAAVAILABLE_SUBSCRIPTION_NAME            = "sbtsub-charges-default-charge-links-dataavailable"

    # Integration
    CREATE_LINKS_REQUEST_QUEUE_NAME                                 = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sbq-create-link-request-name)"
    METERING_POINT_CREATED_TOPIC_NAME                               = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sbt-metering-point-created-name)"
    METERING_POINT_CREATED_SUBSCRIPTION_NAME                        = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sbs-metering-point-created-sub-charges-name)"
    MARKET_PARTICIPANT_CHANGED_TOPIC_NAME                           = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sbt-market-participant-changed-name)"
    MARKET_PARTICIPANT_CHANGED_SUBSCRIPTION_NAME                    = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sbs-market-participant-changed-to-charges-name)"
    
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