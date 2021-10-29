# Use this script to execute the scaffolding of the query stack directly from your solution explorer.
#
# Instructions:
#   1. Build the solution
#   2. Execute this script
#   3. Remember to comment out the following lines in the generated DbContext af scaffolding:
# =======================================================================================================
#            if (!optionsBuilder.IsConfigured)
#            {
##warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
#                optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=ChargesDatabase;Trusted_Connection=True;");
#            }
# =======================================================================================================

# Command arguments
$connectionString = "Server=(localdb)\mssqllocaldb;Database=ChargesDatabase;Trusted_Connection=True;"
$context = "QueryDbContext"
$outputDir = "ScaffoldedModel"
$schema = "charges"

# Make sure DbUp is up2date
Invoke-Expression "dotnet build ..\GreenEnergyHub.Charges.ApplyDBMigrationsApp\GreenEnergyHub.Charges.ApplyDBMigrationsApp.csproj"

# Update database model
Invoke-Expression "dotnet run --project ..\\GreenEnergyHub.Charges.ApplyDBMigrationsApp -- ""Server=(localdb)\mssqllocaldb;Database=ChargesDatabase;Trusted_Connection=True;"" includeSeedData includeTestData"

# Execute scaffold command
$cmd = "dotnet ef dbcontext scaffold ""$connectionString"" Microsoft.EntityFrameworkCore.SqlServer --output-dir $outputDir --context $context --schema $schema --force"
Invoke-Expression $cmd

Write-Warning "Remove or comment out the if-clause in the method 'OnConfiguring' of the generated file '$outputDir\$context.cs'."
