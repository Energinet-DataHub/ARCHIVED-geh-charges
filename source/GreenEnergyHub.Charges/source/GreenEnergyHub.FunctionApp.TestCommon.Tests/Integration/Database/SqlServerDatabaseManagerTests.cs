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
using System.Threading.Tasks;
using FluentAssertions;
using GreenEnergyHub.FunctionApp.TestCommon.Database;
using GreenEnergyHub.FunctionApp.TestCommon.Tests.Fixtures;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GreenEnergyHub.FunctionApp.TestCommon.Tests.Integration.Database
{
    public class SqlServerDatabaseManagerTests
    {
        [Collection(nameof(SqlServerDatabaseCollectionFixture))]
        public class CreateDatabase
        {
            [Fact]
            public async Task When_DbContextIsConfigured_Then_DatabaseCanBeCreatedAsync()
            {
                // Arrange
                var sut = new SqlServerDatabaseManagerSut();

                // Act
                var databaseCreated = await sut.CreateDatabaseAsync();

                // Assert
                databaseCreated.Should().BeTrue();
                await using var context = sut.CreateDbContext();
                (await context.Database.CanConnectAsync()).Should().Be(true);
                await context.Database.EnsureDeletedAsync();
            }

            [Fact]
            public async Task When_DatabaseExists_Then_NothingHappensAndDatabaseStillExists_Async()
            {
                // Arrange
                var sut = new SqlServerDatabaseManagerSut();
                await sut.CreateDatabaseAsync();

                // Act
                var databaseCreated = await sut.CreateDatabaseAsync();

                // Assert
                databaseCreated.Should().BeFalse();
                await using var context = sut.CreateDbContext();
                (await context.Database.CanConnectAsync()).Should().Be(true);
                await context.Database.EnsureDeletedAsync();
            }

            [Fact]
            public void When_DbContextIsConfigured_Then_DatabaseCanBeCreated()
            {
                // Arrange
                var sut = new SqlServerDatabaseManagerSut();

                // Act
                var databaseCreated = sut.CreateDatabase();

                // Assert
                databaseCreated.Should().BeTrue();
                using var context = sut.CreateDbContext();
                context.Database.CanConnect().Should().Be(true);
                context.Database.EnsureDeleted();
            }

            [Fact]
            public void When_DatabaseExists_Then_NothingHappensAndDatabaseStillExists()
            {
                // Arrange
                var sut = new SqlServerDatabaseManagerSut();
                sut.CreateDatabase();

                // Act
                var databaseCreated = sut.CreateDatabase();

                // Assert
                databaseCreated.Should().BeFalse();
                using var context = sut.CreateDbContext();
                context.Database.CanConnect().Should().Be(true);
                context.Database.EnsureDeleted();
            }
        }

        [Collection(nameof(SqlServerDatabaseCollectionFixture))]
        public class DeleteDatabase
        {
            [Fact]
            public async Task When_DatabaseExists_Then_ItCanBeDeletedAsync()
            {
                // Arrange
                var sut = new SqlServerDatabaseManagerSut();
                await sut.CreateDatabaseAsync();

                // Act
                var databaseDeleted = await sut.DeleteDatabaseAsync();

                // Assert
                databaseDeleted.Should().BeTrue();
                await using var context = sut.CreateDbContext();
                (await context.Database.CanConnectAsync()).Should().Be(false);
            }

            [Fact]
            public async Task When_DatabaseDoesNotExist_Then_NothingHappens_Async()
            {
                // Arrange
                var sut = new SqlServerDatabaseManagerSut();
                await sut.DeleteDatabaseAsync();

                // Act
                var databaseDeleted = await sut.DeleteDatabaseAsync();

                // Assert
                databaseDeleted.Should().BeFalse();
                await using var context = sut.CreateDbContext();
                (await context.Database.CanConnectAsync()).Should().Be(false);
            }

            [Fact]
            public void When_DatabaseExists_Then_ItCanBeDeleted()
            {
                // Arrange
                var sut = new SqlServerDatabaseManagerSut();
                sut.CreateDatabase();

                // Act
                var databaseDeleted = sut.DeleteDatabase();

                // Assert
                databaseDeleted.Should().BeTrue();
                using var context = sut.CreateDbContext();
                context.Database.CanConnect().Should().Be(false);
            }

            [Fact]
            public void When_DatabaseDoesNotExist_Then_NothingHappens()
            {
                // Arrange
                var sut = new SqlServerDatabaseManagerSut();
                sut.DeleteDatabase();

                // Act
                var databaseDeleted = sut.DeleteDatabase();

                // Assert
                databaseDeleted.Should().BeFalse();
                using var context = sut.CreateDbContext();
                context.Database.CanConnect().Should().Be(false);
            }
        }

        public class ExampleEntity
        {
            public int Id { get; set; }
        }

        public class ExampleDbContext : DbContext
        {
            public ExampleDbContext(DbContextOptions<ExampleDbContext> options)
                : base(options)
            {
            }

            public DbSet<ExampleEntity>? ExampleEntities { get; set; }
        }

        public class SqlServerDatabaseManagerSut : SqlServerDatabaseManager<ExampleDbContext>
        {
            public SqlServerDatabaseManagerSut()
                : base(nameof(SqlServerDatabaseManagerSut))
            {
            }

            public override ExampleDbContext CreateDbContext()
            {
                var optionsBuilder = new DbContextOptionsBuilder<ExampleDbContext>();
                optionsBuilder.UseSqlServer(ConnectionString);
                return new ExampleDbContext(optionsBuilder.Options);
            }
        }
    }
}
