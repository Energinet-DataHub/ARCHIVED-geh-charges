﻿// Copyright 2020 Energinet DataHub A/S
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
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using FluentAssertions;
using FluentAssertions.Execution;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.Database;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.WebApi;
using GreenEnergyHub.Charges.TestCore.Builders.Query;
using GreenEnergyHub.Charges.TestCore.Data;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.WebApi.V1
{
    [IntegrationTest]
    [Collection(nameof(ChargesWebApiCollectionFixture))]
    public class ChargesControllerTests :
        WebApiTestBase<ChargesWebApiFixture>,
        IClassFixture<ChargesWebApiFixture>,
        IClassFixture<WebApiFactory>
    {
        private const string BaseUrl = "/v1/Charges";
        private readonly ChargesDatabaseManager _databaseManager;

        public ChargesControllerTests(
            ChargesWebApiFixture chargesWebApiFixture,
            ITestOutputHelper testOutputHelper)
            : base(chargesWebApiFixture, testOutputHelper)
        {
            _databaseManager = chargesWebApiFixture.DatabaseManager;
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task GetAsync_WhenChargesExists_ReturnsOkAndCorrectContentType(WebApiFactory factory)
        {
            // Arrange
            var sut = CreateHttpClient(factory);

            // Act
            var response = await sut.GetAsync($"{BaseUrl}/GetAsync");

            // Assert
            var contentType = response.Content.Headers.ContentType!.ToString();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            contentType.Should().Be("application/json; charset=utf-8");
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task GetAsync_WhenRequested_ReturnsChargeInformation(WebApiFactory factory)
        {
            // Arrange
            var sut = CreateHttpClient(factory);

            // Act
            var response = await sut.GetAsync($"{BaseUrl}/GetAsync");

            // Assert
            var jsonString = await response.Content.ReadAsStringAsync();
            var chargesList = JsonSerializer.Deserialize<List<ChargeV1Dto>>(
                jsonString,
                GetJsonSerializerOptions());

            chargesList.Should().HaveCountGreaterThan(0);
            var actual = chargesList!.Single(x => x.ChargeId == "EA-001");
            actual.ChargeType.Should().Be(ChargeType.D03);
            actual.Resolution.Should().Be(Resolution.PT1H);
            actual.ChargeName.Should().Be("Elafgift");
            actual.ChargeDescription.Should().Be("Elafgiften");
            actual.ChargeOwner.Should().Be("5790000432752");
            actual.ChargeOwnerName.Should().Be("System Operator");
            actual.VatClassification.Should().Be(VatClassification.Vat25);
            actual.TaxIndicator.Should().BeTrue();
            actual.TransparentInvoicing.Should().BeTrue();
            actual.ValidFromDateTime.Should().Be(new DateTime(2014, 12, 31, 23, 00, 00));
            actual.ValidToDateTime.Should().Be(null); // because it's year (9999)
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task SearchAsync_WhenSearchCriteriaIsValid_ReturnsOkAndCorrectContentType(
            ChargeSearchCriteriaV1DtoBuilder chargeSearchCriteriaV1DtoBuilder,
            WebApiFactory factory)
        {
            // Arrange
            var sut = CreateHttpClient(factory);
            var searchCriteria = chargeSearchCriteriaV1DtoBuilder.Build();

            // Act
            var response = await sut.PostAsJsonAsync($"{BaseUrl}/SearchAsync", searchCriteria);

            // Assert
            var contentType = response.Content.Headers.ContentType!.ToString();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            contentType.Should().Be("application/json; charset=utf-8");
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task SearchAsync_WhenSearchCriteriaIsNotValid_ReturnsBadRequest(
            ChargeSearchCriteriaV1DtoBuilder chargeSearchCriteriaV1DtoBuilder,
            WebApiFactory factory)
        {
            // Arrange
            var sut = CreateHttpClient(factory);
            var searchCriteria = chargeSearchCriteriaV1DtoBuilder
                .WithOwnerId(Guid.Empty)
                .Build();

            // Act
            var response = await sut.PostAsJsonAsync($"{BaseUrl}/SearchAsync", searchCriteria);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task SearchAsync_WhenRequested_ReturnsChargeV1Dtos(
            ChargeSearchCriteriaV1DtoBuilder chargeSearchCriteriaV1DtoBuilder,
            WebApiFactory factory)
        {
            // Arrange
            var sut = CreateHttpClient(factory);
            var searchCriteria = chargeSearchCriteriaV1DtoBuilder
                .Build();

            // Act
            var response = await sut.PostAsJsonAsync($"{BaseUrl}/SearchAsync", searchCriteria);

            // Assert
            var jsonString = await response.Content.ReadAsStringAsync();
            var chargesList = JsonSerializer.Deserialize<List<ChargeV1Dto>>(
                jsonString,
                GetJsonSerializerOptions());

            using var assertionScope = new AssertionScope();
            chargesList.Should().HaveCountGreaterThan(0);
            chargesList.Should().BeInAscendingOrder(c => c.ChargeName).And
                .ThenBeInDescendingOrder(c => c.ValidFromDateTime);
            var actual = chargesList!.First(c => c.ChargeId == TestData.Charge.TestTar001.SenderProvidedChargeId);
            actual.Resolution.Should().Be(Resolution.PT1H);
            actual.ChargeType.Should().Be(ChargeType.D03);
            actual.ChargeName.Should().Be(TestData.Charge.TestTar001.Name);
            actual.ChargeOwner.Should().Be(TestData.Charge.TestTar001.ChargeOwnerId);
            actual.ChargeOwnerName.Should().NotBeEmpty();
            actual.TaxIndicator.Should().BeFalse();
            actual.TransparentInvoicing.Should().BeTrue();
            actual.ValidFromDateTime.Should().Be(new DateTimeOffset(2021, 12, 31, 23, 00, 00, TimeSpan.Zero));
            actual.ValidToDateTime.Should().Be(new DateTimeOffset(2022, 10, 31, 23, 00, 00, TimeSpan.Zero));
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
            sut.DefaultRequestHeaders.Add("Authorization", $"Bearer xxx");
            return sut;
        }
    }
}
