module "kv_charges" {
  source                          = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//key-vault?ref=1.0.0"
  name                            = "kv${var.project}${var.organisation}${var.environment}"
  resource_group_name             = data.azurerm_resource_group.main.name
  location                        = data.azurerm_resource_group.main.location
  tags                            = data.azurerm_resource_group.main.tags
  enabled_for_template_deployment = true
  sku_name                        = "standard"
  
  access_policy = [
    {
      tenant_id               = var.tenant_id
      object_id               = var.spn_object_id
      secret_permissions      = ["set", "get", "list", "delete"]
      certificate_permissions = []
      key_permissions         = []
      storage_permissions     = []
    }
  ]
}