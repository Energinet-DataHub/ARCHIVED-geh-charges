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
using Azure.Storage.Blobs;
using FluentAssertions;
using GreenEnergyHub.FunctionApp.TestCommon.Azurite;
using GreenEnergyHub.FunctionApp.TestCommon.Tests.Fixtures;
using Xunit;

namespace GreenEnergyHub.FunctionApp.TestCommon.Tests.Integration.Azurite
{
    public class AzuriteManagerTests
    {
        [Collection(nameof(AzuriteCollectionFixture))]
        public class MakeSureAzuriteCanBeStartedTwice
        {
            [Fact]
            public async Task When_AzuriteProcessIsDisposed_Then_ItCanStartAgain()
            {
                // Arrange
                var azuriteManagerToStartFirst = new AzuriteManager();
                try
                {
                    azuriteManagerToStartFirst.StartAzurite();
                }
                finally
                {
                    // Act
                    azuriteManagerToStartFirst.Dispose();
                }

                // Assert
                var azuriteManagerToStartSecond = new AzuriteManager();
                try
                {
                    azuriteManagerToStartSecond.StartAzurite();
                    var exception = await Record.ExceptionAsync(CreateStorageContainer);
                    exception.Should().BeNull();
                }
                finally
                {
                    azuriteManagerToStartSecond.Dispose();
                }
            }

            [Fact]
            public void When_AzuriteProcessIsNotDisposed_Then_ItCanStillStartAgain()
            {
                // Arrange
                var azuriteManagerToStartFirst = new AzuriteManager();
                var azuriteManagerToStartSecond = new AzuriteManager();
                try
                {
                    azuriteManagerToStartFirst.StartAzurite();

                    // Act
                    var exception = Record.Exception(() => azuriteManagerToStartSecond.StartAzurite());

                    // Assert
                    exception.Should().BeNull();
                }
                finally
                {
                    azuriteManagerToStartFirst.Dispose();
                    azuriteManagerToStartSecond.Dispose();
                }
            }

            private async Task CreateStorageContainer()
            {
                var storageConnectionString = "UseDevelopmentStorage=true";
                var containerName = $"Test{Guid.NewGuid()}".ToLower();

                var blobServiceClient = new BlobServiceClient(storageConnectionString);
                var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
                await blobContainerClient.CreateAsync();
            }
        }
    }
}
