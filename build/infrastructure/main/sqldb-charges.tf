data "azurerm_sql_server" "sqlsrv" {
  name                = var.sharedresources_sql_server_name
  resource_group_name = data.azurerm_resource_group.shared_resources.name
}

module "sqldb_charges" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//sql-database?ref=1.7.0"
  name                = "sqldb-${var.project}-${var.organisation}"
  resource_group_name = data.azurerm_resource_group.shared_resources.name
  location            = data.azurerm_resource_group.shared_resources.location
  tags                = data.azurerm_resource_group.shared_resources.tags
  server_name         = data.azurerm_sql_server.sqlsrv.name
}