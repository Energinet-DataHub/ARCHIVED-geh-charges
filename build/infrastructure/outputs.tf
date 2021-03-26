output "sql_server_url" {
  description = "URL for the shared sql server"
  value = module.sqlsrv_charges.fully_qualified_domain_name
  sensitive = true
}

output "charges_sql_database_name" {
  description = "Name of the charges database in the shared sql server"
  value = module.sqldb_charges.name
  sensitive = true
}

output "charges_user_keyvaultname" {
  description = "Name of the secret in the keyvault containing the username for the charges sql database"
  value = module.sqlsrv_admin_username.name
  sensitive = true
}

output "charges_password_keyvaultname" {
  description = "Name of the secret in the keyvault containing the password for the charges sql database"
  value = module.sqlsrv_admin_password.name
  sensitive = true
}

output "kv_charges_name" {
  description = "Name of the keyvault"
  value = module.kv_charges.name
  sensitive = true
}
