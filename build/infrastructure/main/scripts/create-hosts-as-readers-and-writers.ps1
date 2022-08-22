param (
    [Parameter(Mandatory = $false)]
    [string]$sqlServerName,
    [Parameter(Mandatory = $true)]
    [string]$databaseName,
    [Parameter(Mandatory = $false)] # Format must be abc, def, ghi
    [string]$applicationHosts
)

if ([string]::IsNullOrWhiteSpace($sqlServerName))
{
    Write-Host "SQL server name null or whitespace"
    Exit 20
}

if ([string]::IsNullOrWhiteSpace($databaseName))
{
    Write-Host "Database name null or whitespace"
    Exit 21
}

$applicationHostsArray = $applicationHosts.Split(", ",[System.StringSplitOptions]::RemoveEmptyEntries)
if ($applicationHostsArray.count -eq 0)
{
    Write-Host "No application hosts"
    Exit 22
}

Install-Module sqlserver -Force
Import-Module sqlserver

$sqlServerName = "$sqlServerName.database.windows.net"
$accessToken=$(az account get-access-token --resource=https://database.windows.net --query accessToken --output tsv)

foreach ($applicationHostElement in $applicationHostsArray)
{
    $queryCheckapplicationHost = "SELECT [name] FROM [sys].[database_principals] WHERE [name] = N'$applicationHostElement'"
    $result = Invoke-SqlCmd -ServerInstance $sqlServerName -AccessToken $accessToken -Database $databaseName -Query $queryCheckapplicationHost

    if ($null -eq $result)
    {
        $queryCreateDbUser = "CREATE USER [$applicationHostElement] FROM EXTERNAL PROVIDER;"
        Invoke-SqlCmd -ServerInstance $sqlServerName -AccessToken $accessToken -Database $databaseName -Query $queryCreateDbUser
        Write-Host "User $applicationHostElement created"
    }
    
    $queryapplicationHostAsDbReader = "ALTER ROLE db_datareader ADD MEMBER [$applicationHostElement];"
    Invoke-SqlCmd -ServerInstance $sqlServerName -AccessToken $accessToken -Database $databaseName -Query $queryapplicationHostAsDbReader
    
    $queryapplicationHostAsDbWriter = "ALTER ROLE db_datawriter ADD MEMBER [$applicationHostElement];"
    Invoke-SqlCmd -ServerInstance $sqlServerName -AccessToken $accessToken -Database $databaseName -Query $queryapplicationHostAsDbWriter
}

