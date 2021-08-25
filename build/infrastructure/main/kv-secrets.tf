data "azurerm_key_vault_secret" "SHARED_RESOURCES_DB_ADMIN_NAME" {
  name         = "SHARED-RESOURCES-DB-ADMIN-NAME"
  key_vault_id = data.azurerm_key_vault.kv_sharedresources.id
}

data "azurerm_key_vault_secret" "SHARED_RESOURCES_DB_ADMIN_PASSWORD" {
  name         = "SHARED-RESOURCES-DB-ADMIN-PASSWORD"
  key_vault_id = data.azurerm_key_vault.kv_sharedresources.id
}

data "azurerm_key_vault_secret" "SHARED_RESOURCES_DB_URL" {
  name         = "SHARED-RESOURCES-DB-URL"
  key_vault_id = data.azurerm_key_vault.kv_sharedresources.id
}