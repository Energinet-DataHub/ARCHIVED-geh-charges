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

using System.Collections.Generic;
using FluentAssertions;
using GreenEnergyHub.Charges.Application.Charges.Factories;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands;
using GreenEnergyHub.Charges.TestCore.Builders.Command;
using GreenEnergyHub.TestHelpers;
using GreenEnergyHub.TestHelpers.FluentAssertionsExtensions;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.Charges.Factories
{
    [UnitTest]
    public class ChargePriceOperationsAcceptedEventFactoryTests
    {
        [Theory]
        [InlineAutoDomainData]
        public void Create_WhenValidConfirmation_HasNoNullsOrEmptyCollections(
            DocumentDtoBuilder documentDtoBuilder,
            ChargePriceOperationDtoBuilder chargePriceOperationDtoBuilder,
            ChargePriceOperationsAcceptedEventFactory sut)
        {
            // Act
            var document = documentDtoBuilder.Build();
            var operations = new List<ChargePriceOperationDto> { chargePriceOperationDtoBuilder.Build() };
            var actual = sut.Create(document, operations);

            // Assert
            actual.Should().NotContainNullEnumerable();
        }
    }
}
