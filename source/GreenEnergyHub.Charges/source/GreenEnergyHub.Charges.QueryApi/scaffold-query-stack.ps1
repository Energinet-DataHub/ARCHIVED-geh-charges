# Use this script to execute the scaffolding of the query stack directly from your solution explorer.
#
# Instructions:
#   1. Install necessary tools
#          dotnet tool install --global dotnet-ef
#          dotnet tool install --global dotnet-format
#   2. Execute this script
#   3. Remove old scaffolded classes if no longer exist 
# =======================================================================================================

# Command arguments
$connectionString = "Server=(localdb)\mssqllocaldb;Database=ChargesDatabase;Trusted_Connection=True;"
$context = "QueryDbContext"
$outputDir = "Model\Scaffolded"
$schema = "charges"

# Update database model
Invoke-Expression "dotnet build ..\GreenEnergyHub.Charges.ApplyDBMigrationsApp\GreenEnergyHub.Charges.ApplyDBMigrationsApp.csproj"
Invoke-Expression "dotnet run --project ..\\GreenEnergyHub.Charges.ApplyDBMigrationsApp -- ""Server=(localdb)\mssqllocaldb;Database=ChargesDatabase;Trusted_Connection=True;"" includeSeedData includeTestData"

# Update scaffolded model
$cmd = "dotnet ef dbcontext scaffold ""$connectionString"" Microsoft.EntityFrameworkCore.SqlServer --output-dir $outputDir --context $context --schema $schema --force"
Invoke-Expression $cmd

#Write-Warning "Remove or comment out the if-clause in the method 'OnConfiguring' of the generated file '$outputDir\$context.cs'."
