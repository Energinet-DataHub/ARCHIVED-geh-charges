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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;
using Energinet.DataHub.Core.Schemas;
using Energinet.DataHub.Core.SchemaValidation;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Infrastructure.CimDeserialization.ChargeBundle;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Iso8601;
using Moq;
using NodaTime;
using NodaTime.Text;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.CimDeserialization.ChargeBundle
{
    [UnitTest]
    public class ChargeCommandBundleConverterTests
    {
        [Theory]
        [InlineAutoMoqData]
        public async Task ConvertAsync_WhenCalledWithValidCimMessage_ReturnsParsedObject(
            [Frozen] Mock<ICorrelationContext> context,
            [Frozen] Mock<IIso8601Durations> iso8601Durations,
            ChargeCommandBundleConverter sut)
        {
            // Arrange
            var correlationId = Guid.NewGuid().ToString();
            var expectedTime = InstantPattern.ExtendedIso.Parse("2021-01-01T23:00:00Z").Value;
            var reader = GetReaderAndArrangeTest(
                context,
                iso8601Durations,
                correlationId,
                expectedTime,
                "GreenEnergyHub.Charges.Tests.TestFiles.Syntax_Valid_CIM_Charge.xml");

            // Act
            var actualBundle = (ChargeInformationCommandBundle)await sut.ConvertAsync(reader).ConfigureAwait(false);

            // Assert
            var actual = actualBundle.Commands.Single();

            // Document
            actual.Document.Id.Should().Be("25369874");
            actual.Document.Type.Should().Be(DocumentType.RequestChangeOfPriceList);
            actual.Document.BusinessReasonCode.Should().Be(BusinessReasonCode.UpdateChargeInformation);
            actual.Document.Sender.MarketParticipantId.Should().Be("5799999925698");
            actual.Document.Sender.BusinessProcessRole.Should().Be(MarketParticipantRole.GridAccessProvider);
            actual.Document.Recipient.MarketParticipantId.Should().Be("5790001330552");
            actual.Document.Recipient.BusinessProcessRole.Should().Be(MarketParticipantRole.MeteringPointAdministrator);
            actual.Document.CreatedDateTime.Should().Be(InstantPattern.ExtendedIso.Parse("2021-12-17T09:30:47Z").Value);

            // Charge operation
            var actualChargeOperation = actual.Operations.First();
            actualChargeOperation.Id.Should().Be("36251478");
            actualChargeOperation.ChargeOwner.Should().Be("5799999925698");
            actualChargeOperation.Type.Should().Be(ChargeType.Tariff);
            actualChargeOperation.ChargeId.Should().Be("253C");
            actualChargeOperation.ChargeName.Should().Be("Elafgift 2019");
            actualChargeOperation.ChargeDescription.Should().Be("Dette er elafgiftssatsten for 2019");
            actualChargeOperation.Resolution.Should().Be(Resolution.PT1H);
            actualChargeOperation.StartDateTime.Should()
                .Be(InstantPattern.ExtendedIso.Parse("2020-12-17T23:00:00Z").Value);
            actualChargeOperation.EndDateTime.Should()
                .Be(InstantPattern.ExtendedIso.Parse("2031-12-17T23:00:00Z").Value);
            actualChargeOperation.VatClassification.Should().Be(VatClassification.Vat25);
            actualChargeOperation.TransparentInvoicing.Should().Be(TransparentInvoicing.Transparent);
            actualChargeOperation.TaxIndicator.Should().Be(TaxIndicator.Tax);

            // Points
            actualChargeOperation.Points.Should().HaveCount(2);
            actualChargeOperation.Points[0].Position.Should().Be(1);
            actualChargeOperation.Points[0].Time.Should().Be(expectedTime);
            actualChargeOperation.Points[0].Price.Should().Be(100m);
            actualChargeOperation.Points[1].Position.Should().Be(2);
            actualChargeOperation.Points[1].Time.Should().Be(expectedTime);
            actualChargeOperation.Points[1].Price.Should().Be(200m);

            // Verify Iso8601Durations was used correctly
            iso8601Durations.Verify(
                i => i.GetTimeFixedToDuration(
                    expectedTime,
                    "PT1H",
                    0),
                Times.Once);

            iso8601Durations.Verify(
                i => i.GetTimeFixedToDuration(
                    expectedTime,
                    "PT1H",
                    1),
                Times.Once);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task ConvertAsync_WhenCalledWithValidCimMessageWithoutPrices_ReturnsParsedObject(
            [Frozen] Mock<ICorrelationContext> context,
            [Frozen] Mock<IIso8601Durations> iso8601Durations,
            ChargeCommandBundleConverter sut)
        {
            // Arrange
            var correlationId = Guid.NewGuid().ToString();
            var expectedTime = InstantPattern.ExtendedIso.Parse("2021-04-17T22:00:00Z").Value;
            var reader = GetReaderAndArrangeTest(
                context,
                iso8601Durations,
                correlationId,
                expectedTime,
                "GreenEnergyHub.Charges.Tests.TestFiles.Valid_CIM_Charge_Without_Prices.xml");

            // Act
            var actualBundle = (ChargeInformationCommandBundle)await sut.ConvertAsync(reader).ConfigureAwait(false);

            // Assert
            var actual = actualBundle.Commands.Single();

            // Charge operation
            var actualChargeOperation = actual.Operations.First();
            actualChargeOperation.Id.Should().Be("36251479");
            actualChargeOperation.ChargeOwner.Should().Be("5799999925699");
            actualChargeOperation.Type.Should().Be(ChargeType.Fee);
            actualChargeOperation.ChargeId.Should().Be("888");
            actualChargeOperation.ChargeName.Should().Be("Test 888");
            actualChargeOperation.ChargeDescription.Should().Be("Description 888");
            actualChargeOperation.Resolution.Should().Be(Resolution.PT15M);
            actualChargeOperation.StartDateTime.Should().Be(expectedTime);
            actualChargeOperation.EndDateTime.Should().BeNull();
            actualChargeOperation.VatClassification.Should().Be(VatClassification.NoVat);
            actualChargeOperation.TransparentInvoicing.Should().Be(TransparentInvoicing.NonTransparent);
            actualChargeOperation.TaxIndicator.Should().Be(TaxIndicator.NoTax);

            // Prices, should not be any
            actualChargeOperation.Points.Should().BeEmpty();
            iso8601Durations.Verify(
                i => i.GetTimeFixedToDuration(
                    It.IsAny<Instant>(),
                    It.IsAny<string>(),
                    It.IsAny<int>()),
                Times.Never);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task ConvertAsync_WhenCalledWithValidCimMessageWithPricesWithoutMasterData_ReturnsParsedObject(
            [Frozen] Mock<ICorrelationContext> context,
            [Frozen] Mock<IIso8601Durations> iso8601Durations,
            ChargeCommandBundleConverter sut)
        {
            // Arrange
            var correlationId = Guid.NewGuid().ToString();
            var expectedTime = InstantPattern.ExtendedIso.Parse("2020-12-31T23:00:00Z").Value;
            var expectedStartInterval = InstantPattern.ExtendedIso.Parse("2020-12-31T23:00:00Z").Value;
            var expectedEndInterval = InstantPattern.ExtendedIso.Parse("2021-02-28T23:00:00Z").Value;
            var reader = GetReaderAndArrangeTest(
                context,
                iso8601Durations,
                correlationId,
                expectedTime,
                "GreenEnergyHub.Charges.Tests.TestFiles.Valid_CIM_Charge_Prices_Without_Master_Data.xml");

            // Act
            var actualBundle = (ChargePriceCommandBundle)await sut.ConvertAsync(reader).ConfigureAwait(false);

            // Assert
            var actual = actualBundle.Commands.Single();

            // Charge operation, should only be partially filled
            var actualChargeOperation = actual.Operations.First();
            actualChargeOperation.Id.Should().Be("36251480");
            actualChargeOperation.ChargeOwner.Should().Be("5799999925600");
            actualChargeOperation.Type.Should().Be(ChargeType.Subscription);
            actualChargeOperation.ChargeId.Should().Be("444");
            actualChargeOperation.PointsStartInterval.Should().Be(expectedStartInterval);
            actualChargeOperation.PointsEndInterval.Should().Be(expectedEndInterval);

            // Points
            actualChargeOperation.Points.Should().HaveCount(2);
            actualChargeOperation.Points[0].Position.Should().Be(1);
            actualChargeOperation.Points[0].Time.Should().Be(expectedTime);
            actualChargeOperation.Points[0].Price.Should().Be(0.536m);
            actualChargeOperation.Points[1].Position.Should().Be(2);
            actualChargeOperation.Points[1].Time.Should().Be(expectedTime);
            actualChargeOperation.Points[1].Price.Should().Be(14.984m);

            // Verify Iso8601Durations was used correctly
            iso8601Durations.Verify(
                i => i.GetTimeFixedToDuration(
                    expectedTime,
                    "P1M",
                    0),
                Times.Once);

            iso8601Durations.Verify(
                i => i.GetTimeFixedToDuration(
                    expectedTime,
                    "P1M",
                    1),
                Times.Once);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task ConvertAsync_WhenCalledWithValidBundle_ReturnsMultipleParsedObject(
            [Frozen] Mock<ICorrelationContext> context,
            [Frozen] Mock<IIso8601Durations> iso8601Durations,
            ChargeCommandBundleConverter sut)
        {
            // Arrange
            var correlationId = Guid.NewGuid().ToString();
            var expectedTime = InstantPattern.ExtendedIso.Parse("2022-10-31T23:00:00Z").Value;
            var reader = GetReaderAndArrangeTest(
                context,
                iso8601Durations,
                correlationId,
                expectedTime,
                "GreenEnergyHub.Charges.Tests.TestFiles.CreateTariffsBundle.xml");

            // Act
            var actual = (ChargeInformationCommandBundle)await sut.ConvertAsync(reader).ConfigureAwait(false);

            // Assert

            // Charge operation
            var actualFirstChargeCommand = actual.Commands.Single(x => x.Operations.Any(y => y.Id == "36251480"));
            var actualFirstChargeOperationDto = actualFirstChargeCommand.Operations.First();
            actualFirstChargeOperationDto.ChargeOwner.Should().Be("8100000000030");
            actualFirstChargeOperationDto.Type.Should().Be(ChargeType.Tariff);
            actualFirstChargeOperationDto.ChargeId.Should().Be("ChId1234567890");
            actualFirstChargeOperationDto.ChargeName.Should().Be("Charge Tariff day Name 1");
            actualFirstChargeOperationDto.ChargeDescription.Should().Be("The charge description 1");
            actualFirstChargeOperationDto.Resolution.Should().Be(Resolution.P1D);
            actualFirstChargeOperationDto.StartDateTime.Should().Be(expectedTime);
            actualFirstChargeOperationDto.EndDateTime.Should().BeNull();
            actualFirstChargeOperationDto.VatClassification.Should().Be(VatClassification.NoVat);
            actualFirstChargeOperationDto.TransparentInvoicing.Should().Be(TransparentInvoicing.NonTransparent);
            actualFirstChargeOperationDto.TaxIndicator.Should().Be(TaxIndicator.Tax);

            // Prices
            actualFirstChargeOperationDto.Points.Should().HaveCount(1);
            actualFirstChargeOperationDto.Points.First().Price.Should().Be(150.001m);

            var actualSecondChargeCommand = actual.Commands.Single(x => x.Operations.Any(y => y.Id == "36251481"));
            var actualSecondChargeOperationDto = actualSecondChargeCommand.Operations.First();
            actualSecondChargeOperationDto.ChargeOwner.Should().Be("8100000000030");
            actualSecondChargeOperationDto.Type.Should().Be(ChargeType.Tariff);
            actualSecondChargeOperationDto.ChargeId.Should().Be("ChId1234567891");
            actualSecondChargeOperationDto.ChargeName.Should().Be("Charge Tariff day Name 2");
            actualSecondChargeOperationDto.ChargeDescription.Should().Be("The charge description 2");
            actualSecondChargeOperationDto.Resolution.Should().Be(Resolution.P1D);
            actualSecondChargeOperationDto.StartDateTime.Should().Be(expectedTime);
            actualSecondChargeOperationDto.EndDateTime.Should().BeNull();
            actualSecondChargeOperationDto.VatClassification.Should().Be(VatClassification.Vat25);
            actualSecondChargeOperationDto.TransparentInvoicing.Should().Be(TransparentInvoicing.Transparent);
            actualSecondChargeOperationDto.TaxIndicator.Should().Be(TaxIndicator.NoTax);

            // Prices
            actualSecondChargeOperationDto.Points.Should().HaveCount(1);
            actualSecondChargeOperationDto.Points.First().Price.Should().Be(200.001m);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task ConvertAsync_WhenCalledWithMixedChargeBundle_ReturnsGroupedChargeCommands(
            [Frozen] Mock<ICorrelationContext> context,
            [Frozen] Mock<IIso8601Durations> iso8601Durations,
            ChargeCommandBundleConverter sut)
        {
            // Arrange
            var correlationId = Guid.NewGuid().ToString();
            var expectedTime = InstantPattern.ExtendedIso.Parse("2022-10-31T23:00:00Z").Value;
            var reader = GetReaderAndArrangeTest(
                context,
                iso8601Durations,
                correlationId,
                expectedTime,
                "GreenEnergyHub.Charges.Tests.TestFiles.BundleMixOfChargeMasterDataOperations.xml");

            // Act
            var actual = (ChargeInformationCommandBundle)await sut.ConvertAsync(reader).ConfigureAwait(false);

            // Assert - Grouping of operations for same unique charge is correctly done
            actual.Commands.Should().HaveCount(3);
            var chargeCommandWithTwoOperations = actual.Commands.Single(x =>
                x.Operations.Any(y => y.Id == "Operation1"));
            chargeCommandWithTwoOperations.Operations.Should().HaveCount(2);
            chargeCommandWithTwoOperations.Operations.Should().Contain(x => x.Id == "Operation1");
            chargeCommandWithTwoOperations.Operations.Should().Contain(x => x.Id == "Operation4");
        }

        private static SchemaValidatingReader GetReaderAndArrangeTest(
            Mock<ICorrelationContext> context,
            Mock<IIso8601Durations> iso8601Durations,
            string correlationId,
            Instant expectedTime,
            string embeddedFile)
        {
            context.Setup(c => c.Id).Returns(correlationId);

            iso8601Durations.Setup(
                    i => i.GetTimeFixedToDuration(
                        It.IsAny<Instant>(),
                        It.IsAny<string>(),
                        It.IsAny<int>()))
                .Returns(expectedTime);

            var stream = GetEmbeddedResource(embeddedFile);
            return new SchemaValidatingReader(stream, Schemas.CimXml.StructureRequestChangeOfPriceList);
        }

        private static Stream GetEmbeddedResource(string path)
        {
            var assembly = Assembly.GetExecutingAssembly();
            return EmbeddedStreamHelper.GetInputStream(assembly, path);
        }
    }
}
