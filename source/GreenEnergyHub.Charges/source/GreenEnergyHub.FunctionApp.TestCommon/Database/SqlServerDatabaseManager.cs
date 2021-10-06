// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Polly;

namespace GreenEnergyHub.FunctionApp.TestCommon.Database
{
    /// <summary>
    /// An abstract database manager for creating a database for integration tests.
    ///
    /// Ensures:
    ///  * The database is created similar to what we expect in a production environment (e.g. collation).
    ///  * Each manager instance has an unique database instance (connection string).
    ///  * Creation of the DbContext should be similar to production code (must be ensured by implementing <see cref="CreateDbContext"/>).
    /// </summary>
    public abstract class SqlServerDatabaseManager<TContextImplementation>
        where TContextImplementation : DbContext
    {
        private const string CollationName = "SQL_Danish_Pref_CP1_CI_AS";

        protected SqlServerDatabaseManager(string prefixForDatabaseName)
        {
            if (string.IsNullOrWhiteSpace(prefixForDatabaseName))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(prefixForDatabaseName));
            }

            ConnectionString = BuildConnectionString(prefixForDatabaseName);
        }

        public string ConnectionString { get; }

        /// <summary>
        /// IMPORTANT: Dispose the DbContext after use to return the underlying connection to the pool.
        /// </summary>
        public abstract TContextImplementation CreateDbContext();

        /// <summary>
        /// IMPORTANT: The database is not migrated. The intention is to create a new database
        /// with the full schema and <see cref="DeleteDatabaseAsync"/> after testing is done.
        /// Note: Attempts to create a database with a name already existing, will do nothing.
        /// </summary>
        public async Task<bool> CreateDatabaseAsync()
        {
            await using var context = CreateDbContext();
            CreateLocalDatabaseWithoutSchema(context);
            return await context.Database.EnsureCreatedAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// IMPORTANT: The database is not migrated. The intention is to create a new database
        /// with the full schema and <see cref="DeleteDatabase"/> after testing is done.
        /// Note: Attempts to create a database with a name already existing, will do nothing.
        /// </summary>
        public bool CreateDatabase()
        {
            using var context = CreateDbContext();
            CreateLocalDatabaseWithoutSchema(context);
            return context.Database.EnsureCreated();
        }

        /// <summary>
        /// The intention is to delete previously created database using this, when testing is done.
        /// Note: Deletes database created by <see cref="CreateDatabaseAsync"/>
        /// </summary>
        public async Task<bool> DeleteDatabaseAsync()
        {
            await using var context = CreateDbContext();
            return await context.Database.EnsureDeletedAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// The intention is to delete previously created database using this, when testing is done.
        /// Note: Deletes database created by <see cref="CreateDatabase"/>
        /// </summary>
        public bool DeleteDatabase()
        {
            using var context = CreateDbContext();
            return context.Database.EnsureDeleted();
        }

        /// <summary>
        /// We want to create the local SQL database for integration tests so it is similar to the Azure SQL database
        /// with regards to collation and other relevant properties. EF Core currently does not support collation as
        /// part of creation, so we must first create an empty database with the correct collation and then apply the
        /// schema using EF Core.
        ///
        /// Connect to master database and create a database without schema.
        /// </summary>
        private static void CreateLocalDatabaseWithoutSchema(TContextImplementation context)
        {
            // Overview of all exception numbers: https://docs.microsoft.com/en-us/previous-versions/sql/sql-server-2008-r2/cc645603(v=sql.105)?redirectedfrom=MSDN
            const int dbNameAlreadyExistsExceptionNumber = 1801;
            var databaseName = GetDatabaseName(context);
            var createDatabaseCommandText = BuildCreateDatabaseCommandText(databaseName);

            // Retry a specified number of times, using a function to
            // calculate the duration to wait between retries based on
            // the current retry attempt (allows for exponential backoff)
            // In this case will wait for
            //  2 ^ 1 = 2 seconds then
            //  2 ^ 2 = 4 seconds then
            //  2 ^ 3 = 8 seconds then
            //  2 ^ 4 = 16 seconds then
            //  2 ^ 5 = 32 seconds
            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetry(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

            var cancellationToken = new CancellationTokenSource();
            var policyContext = new Context("RetryContext")
            {
                { "CancellationTokenSource", cancellationToken },
            };

            // More than 30-something connections to the master database can exhaust the connection pool, so we retry a few times.
            retryPolicy.Execute(
                (ctx, ct) =>
                {
                    using var masterDbConnection = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;");
                    using var command = new SqlCommand(createDatabaseCommandText, masterDbConnection);
                    masterDbConnection.Open();
                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (SqlException sqlException)
                    {
                        if (sqlException.Number == dbNameAlreadyExistsExceptionNumber)
                        {
                            // Cancel the retry policy if database name already exists.
                            cancellationToken.Cancel();
                        }
                    }
                },
                policyContext,
                cancellationToken.Token);
        }

        /// <summary>
        /// Get the database name from the connection string in the DbContext.
        /// </summary>
        private static string GetDatabaseName(TContextImplementation context)
        {
            return context.Database.GetDbConnection().Database;
        }

        /// <summary>
        /// Create a clean database using SQL, similar to the PowerShell script described in the Development.md file.
        /// </summary>
        private static string BuildCreateDatabaseCommandText(string databaseName)
        {
            return
                $"CREATE DATABASE [{databaseName}] COLLATE {CollationName};" +
                $"ALTER DATABASE [{databaseName}] SET READ_COMMITTED_SNAPSHOT ON;";
        }

        /// <summary>
        /// We create a unique database name to ensure tests using this fixture has their own database.
        /// </summary>
        private static string BuildConnectionString(string prefixForDatabaseName)
        {
            var databaseName = $"{prefixForDatabaseName}Database_{Guid.NewGuid()}";

            // If Connection Timeout=3 is not set, tests that connect to a database that don't exists will take about 10 sec.
            return $"Data Source=(LocalDB)\\MSSQLLocalDB;Integrated Security=true;Database={databaseName};Connection Timeout=3";
        }
    }
}
