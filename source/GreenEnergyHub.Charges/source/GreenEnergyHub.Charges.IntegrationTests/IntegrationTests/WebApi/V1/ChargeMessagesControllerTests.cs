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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Energinet.DataHub.Charges.Contracts.Charge;
using Energinet.DataHub.Charges.Contracts.ChargeMessage;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Infrastructure.Persistence;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.Database;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.WebApi;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.TestCore.Builders.Command;
using GreenEnergyHub.Charges.TestCore.Builders.Query;
using GreenEnergyHub.Charges.TestCore.Data;
using Microsoft.EntityFrameworkCore;
using NodaTime.Extensions;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;
using ChargeType = GreenEnergyHub.Charges.Domain.Charges.ChargeType;
using JsonSerializerOptions = System.Text.Json.JsonSerializerOptions;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.WebApi.V1
{
    [IntegrationTest]
    [Collection(nameof(ChargesWebApiCollectionFixture))]
    public class ChargeMessagesControllerTests :
        WebApiTestBase<ChargesWebApiFixture>,
        IClassFixture<ChargesWebApiFixture>,
        IClassFixture<WebApiFactory>
    {
        private const string BaseUrl = "/v1/ChargeMessages";
        private const string ChargeId = "TestTariff";
        private const string ChargeMessageId = "b263c2c6-9d35-4ef5-9658-4e45f4acaf4c";
        private readonly ChargesDatabaseManager _databaseManager;

        public ChargeMessagesControllerTests(
            ChargesWebApiFixture chargesWebApiFixture,
            ITestOutputHelper testOutputHelper)
            : base(chargesWebApiFixture, testOutputHelper)
        {
            _databaseManager = chargesWebApiFixture.DatabaseManager;
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task SearchAsync_WhenChargeMessagesExists_ReturnsOkAndCorrectContentType(
            WebApiFactory factory)
        {
            // Arrange
            await using var chargesDbContext = _databaseManager.CreateDbContext();
            var charge = await GetChargeAsync(chargesDbContext);

            var sut = CreateHttpClient(factory);
            var searchCriteria = new ChargeMessagesSearchCriteriaV1DtoBuilder().WithChargeId(charge.Id).Build();

            // Act
            var response = await sut.PostAsJsonAsync($"{BaseUrl}/SearchAsync", searchCriteria);

            // Assert
            var contentType = response.Content.Headers.ContentType!.ToString();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            contentType.Should().Be("application/json; charset=utf-8");
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task SearchAsync_WhenRequested_ReturnsChargeMessage(WebApiFactory factory)
        {
            // Arrange
            await using var chargesDbContext = _databaseManager.CreateDbContext();
            var charge = await GetChargeAsync(chargesDbContext);
            var expectedChargeMessage = await GetSeededChargeMessageAsync(chargesDbContext);

            var sut = CreateHttpClient(factory);
            var searchCriteria = new ChargeMessagesSearchCriteriaV1DtoBuilder().WithChargeId(charge.Id).Build();

            // Act
            var response = await sut.PostAsJsonAsync($"{BaseUrl}/SearchAsync", searchCriteria);

            // Assert
            var jsonString = await response.Content.ReadAsStringAsync();
            var chargeMessageV1Dtos = JsonSerializer.Deserialize<List<ChargeMessageV1Dto>>(
                jsonString,
                GetJsonSerializerOptions())!;

            var actual = chargeMessageV1Dtos.Single();

            actual.MessageId.Should().Be(expectedChargeMessage.MessageId);
            actual.MessageType.Should().Be(ChargeMessageDocumentType.D10);

            // actual.MessageDateTime.Should().Be(expectedChargeMessage.MessageDateTime.ToDateTimeOffset());
        }

        private static JsonSerializerOptions GetJsonSerializerOptions()
        {
            return new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() },
            };
        }

        private static HttpClient CreateHttpClient(WebApiFactory factory)
        {
            var sut = factory.CreateClient();
            factory.ReconfigureJwtTokenValidatorMock(isValid: true);
            sut.DefaultRequestHeaders.Add("Authorization", "Bearer xxx");
            return sut;
        }

        private static ChargeMessage CreateChargeMessage()
        {
            return new ChargeMessageBuilder()
                .WithSenderProvidedChargeId(ChargeId)
                .WithChargeType(ChargeType.Tariff)
                .WithMarketParticipantId(SeededData.MarketParticipants.Provider8100000000030.Gln)
                .Build();
        }

        private static async Task AddChargeMessageDataAsync(IChargesDatabaseContext context, ChargeMessage message)
        {
            context.ChargeMessages.Add(message);
            await context.SaveChangesAsync();
        }

        private static async Task<Charge> GetChargeAsync(IChargesDatabaseContext context)
        {
            return await context.Charges
                .SingleAsync(c => c.SenderProvidedChargeId == ChargeId
                                  && c.OwnerId == SeededData.MarketParticipants.Provider8100000000030.Id
                                  && c.Type == ChargeType.Tariff);
        }

        private static async Task<ChargeMessage> GetSeededChargeMessageAsync(IChargesDatabaseContext context)
        {
            return await context.ChargeMessages.SingleAsync(cm => cm.Id == Guid.Parse(ChargeMessageId));
        }
    }
}
