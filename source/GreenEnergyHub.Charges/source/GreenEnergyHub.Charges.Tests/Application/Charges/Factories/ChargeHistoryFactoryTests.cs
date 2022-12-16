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

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using GreenEnergyHub.Charges.Application.Charges.Factories;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands;
using GreenEnergyHub.Charges.TestCore.Builders.Command;
using GreenEnergyHub.Charges.TestCore.TestHelpers;
using GreenEnergyHub.TestHelpers;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.Charges.Factories
{
    [UnitTest]
    public class ChargeHistoryFactoryTests
    {
        [Theory]
        [InlineAutoDomainData]
        public void Create_ChargeInformationOperationsAcceptedEvent_ReturnsChargeHistory(
            ChargeInformationOperationsAcceptedEventBuilder acceptedEventBuilder,
            ChargeInformationOperationDtoBuilder operationDtoBuilder,
            ChargeHistoryFactory sut)
        {
            // Arrange
            var operation = operationDtoBuilder
                .WithTaxIndicator(TaxIndicator.NoTax)
                .WithTransparentInvoicing(TransparentInvoicing.Transparent)
                .WithVatClassification(VatClassification.Vat25)
                .WithEndDateTime(InstantHelper.GetTodayAtMidnightUtc())
                .Build();

            var acceptedEvent = acceptedEventBuilder
                .WithOperations(new List<ChargeInformationOperationDto> { operation })
                .Build();

            // Act
            var chargeHistories = sut.Create(acceptedEvent);

            // Assert
            var actual = chargeHistories.First();
            actual.SenderProvidedChargeId.Should().Be(acceptedEvent.Operations.First().SenderProvidedChargeId);
            actual.Name.Should().Be(acceptedEvent.Operations.First().ChargeName);
            actual.Description.Should().Be(acceptedEvent.Operations.First().ChargeDescription);
            actual.Owner.Should().Be(acceptedEvent.Operations.First().ChargeOwner);
            actual.Resolution.Should().Be(acceptedEvent.Operations.First().Resolution);
            actual.Type.Should().Be(acceptedEvent.Operations.First().ChargeType);
            actual.TaxIndicator.Should().Be(false);
            actual.TransparentInvoicing.Should().Be(true);
            actual.VatClassification.Should().Be(VatClassification.Vat25);
            actual.StartDateTime.Should().Be(acceptedEvent.Operations.First().StartDateTime);
            actual.EndDateTime.Should().Be(acceptedEvent.Operations.First().EndDateTime);
            actual.AcceptedDateTime.Should().Be(acceptedEvent.PublishedTime);
        }

        [Theory]
        [InlineAutoDomainData]
        public void Create_EndDateTimeInAcceptedEventIsNull_ShouldReturnItAsEndDefault(
            ChargeInformationOperationsAcceptedEventBuilder acceptedEventBuilder,
            ChargeInformationOperationDtoBuilder operationDtoBuilder,
            ChargeHistoryFactory sut)
        {
            // Arrange
            var operation = operationDtoBuilder.WithEndDateTime(null).Build();

            var acceptedEvent = acceptedEventBuilder
                .WithOperations(new List<ChargeInformationOperationDto> { operation })
                .Build();

            // Act
            var actual = sut.Create(acceptedEvent);

            // Assert
            actual.First().EndDateTime.Should().NotBeNull();
            actual.First().EndDateTime.Should().Be(InstantExtensions.GetEndDefault());
        }
    }
}
