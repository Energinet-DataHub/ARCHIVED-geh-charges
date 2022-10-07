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
using Energinet.Charges.Contracts.Charge;
using FluentAssertions;
using GreenEnergyHub.Charges.QueryApi.Validation;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.TestCore.Builders.Query;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.WebApi.Validation
{
    [UnitTest]
    public class SearchCriteriaValidatorTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void Validate_WhenNoSearchCriteriaIsSet_ReturnTrue(
            SearchCriteriaDtoBuilder searchCriteriaDtoBuilder)
        {
            // Arrange
            var searchCriteria = searchCriteriaDtoBuilder.Build();

            // Act
            var actual = SearchCriteriaValidator.Validate(searchCriteria);

            // Arrange
            actual.Should().Be(true);
        }

        [Theory]
        [InlineAutoMoqData]
        public void Validate_WhenSearchCriteriaIsSetCorrect_ReturnTrue(
            SearchCriteriaDtoBuilder searchCriteriaDtoBuilder)
        {
            // Arrange
            var searchCriteria = searchCriteriaDtoBuilder
                .WithChargeType(ChargeType.D01)
                .WithChargeIdOrName("test")
                .WithOwnerId(Guid.NewGuid())
                .Build();

            // Act
            var actual = SearchCriteriaValidator.Validate(searchCriteria);

            // Arrange
            actual.Should().Be(true);
        }

        [Theory]
        [InlineAutoMoqData]
        public void Validate_WhenSearchCriteriaOwnerIdIsAEmptyGuid_ReturnsFalse(
            SearchCriteriaDtoBuilder searchCriteriaDtoBuilder)
        {
            // Arrange
            var searchCriteria = searchCriteriaDtoBuilder
                .WithOwnerId(Guid.Empty)
                .Build();

            // Act
            var actual = SearchCriteriaValidator.Validate(searchCriteria);

            // Arrange
            actual.Should().Be(false);
        }

        [Theory]
        [InlineAutoMoqData]
        public void Validate_WhenSearchCriteriaOwnerIdsIsNull_ReturnsTrue(
            SearchCriteriaDtoBuilder searchCriteriaDtoBuilder)
        {
            // Arrange
            var searchCriteria = searchCriteriaDtoBuilder
                .WithOwnerIds(null!)
                .Build();

            // Act
            var actual = SearchCriteriaValidator.Validate(searchCriteria);

            // Arrange
            actual.Should().Be(true);
        }

        [Theory]
        [InlineAutoMoqData]
        public void Validate_WhenSearchCriteriaChargeTypesWithWrongType_ReturnFalse(
            SearchCriteriaDtoBuilder searchCriteriaDtoBuilder)
        {
            // Arrange
            var searchCriteria = searchCriteriaDtoBuilder
                .WithChargeType((ChargeType)100)
                .Build();

            // Act
            var actual = SearchCriteriaValidator.Validate(searchCriteria);

            // Arrange
            actual.Should().Be(false);
        }

        [Theory]
        [InlineAutoMoqData]
        public void Validate_WhenSearchCriteriaChargeTypesWithWrongTypes_ReturnFalse(
            SearchCriteriaDtoBuilder searchCriteriaDtoBuilder)
        {
            // Arrange
            var searchCriteria = searchCriteriaDtoBuilder
                .WithChargeTypes(new List<ChargeType> { (ChargeType)100, (ChargeType)200 })
                .Build();

            // Act
            var actual = SearchCriteriaValidator.Validate(searchCriteria);

            // Arrange
            actual.Should().Be(false);
        }

        [Theory]
        [InlineAutoMoqData]
        public void Validate_WhenSearchCriteriaChargeTypesWithCorrectChargeTypes_ReturnTrue(
            SearchCriteriaDtoBuilder searchCriteriaDtoBuilder)
        {
            // Arrange
            var searchCriteria = searchCriteriaDtoBuilder
                .WithChargeTypes(new List<ChargeType> { ChargeType.D01, ChargeType.D02 })
                .Build();

            // Act
            var actual = SearchCriteriaValidator.Validate(searchCriteria);

            // Arrange
            actual.Should().Be(true);
        }
    }
}
