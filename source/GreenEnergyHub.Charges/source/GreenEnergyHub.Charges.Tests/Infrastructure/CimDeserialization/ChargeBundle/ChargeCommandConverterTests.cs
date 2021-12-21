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
using System.Xml;
using AutoFixture.Xunit2;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Infrastructure.CimDeserialization.ChargeBundle;
using GreenEnergyHub.Charges.Infrastructure.Core.Correlation;
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
    public class ChargeCommandConverterTests
    {
        [Theory]
        [InlineAutoMoqData]
        public async Task ConvertAsync_WhenCalledWithValidCimMessage_ReturnsParsedObject(
            [Frozen] Mock<ICorrelationContext> context,
            [Frozen] Mock<IIso8601Durations> iso8601Durations,
            ChargeCommandConverter sut)
        {
            // Arrange
            var correlationId = Guid.NewGuid().ToString();
            var expectedTime = InstantPattern.ExtendedIso.Parse("2021-01-01T23:00:00Z").Value;
            using var reader = GetReaderAndArrangeTest(
                context,
                iso8601Durations,
                correlationId,
                expectedTime,
                "GreenEnergyHub.Charges.Tests.TestFiles.Syntax_Valid_CIM_Charge.xml");

            // Act
            var actualBundle = (ChargeCommandBundle)await sut.ConvertAsync(reader).ConfigureAwait(false);

            // Assert
            var actual = actualBundle.ChargeCommands.Single();

            // Document
            actual.Document.Id.Should().Be("25369874");
            actual.Document.Type.Should().Be(DocumentType.RequestUpdateChargeInformation);
            actual.Document.BusinessReasonCode.Should().Be(BusinessReasonCode.UpdateChargeInformation);
            actual.Document.Sender.Id.Should().Be("5799999925698");
            actual.Document.Sender.BusinessProcessRole.Should().Be(MarketParticipantRole.GridAccessProvider);
            actual.Document.Recipient.Id.Should().Be("5790001330552");
            actual.Document.Recipient.BusinessProcessRole.Should().Be(MarketParticipantRole.MeteringPointAdministrator);
            actual.Document.CreatedDateTime.Should().Be(InstantPattern.ExtendedIso.Parse("2021-12-17T09:30:47Z").Value);

            // Charge operation
            actual.ChargeOperation.Id.Should().Be("36251478");
            actual.ChargeOperation.ChargeOwner.Should().Be("5799999925698");
            actual.ChargeOperation.Type.Should().Be(ChargeType.Tariff);
            actual.ChargeOperation.ChargeId.Should().Be("253C");
            actual.ChargeOperation.ChargeName.Should().Be("Elafgift 2019");
            actual.ChargeOperation.ChargeDescription.Should().Be("Dette er elafgiftssatsten for 2019");
            actual.ChargeOperation.Resolution.Should().Be(Resolution.PT1H);
            actual.ChargeOperation.StartDateTime.Should().Be(InstantPattern.ExtendedIso.Parse("2020-12-17T23:00:00Z").Value);
            actual.ChargeOperation.EndDateTime.Should()
                .Be(InstantPattern.ExtendedIso.Parse("2031-12-17T23:00:00Z").Value);
            actual.ChargeOperation.VatClassification.Should().Be(VatClassification.Vat25);
            actual.ChargeOperation.TransparentInvoicing.Should().BeTrue();
            actual.ChargeOperation.TaxIndicator.Should().BeTrue();

            // Points
            actual.ChargeOperation.Points.Should().HaveCount(2);
            actual.ChargeOperation.Points[0].Position.Should().Be(1);
            actual.ChargeOperation.Points[0].Time.Should().Be(expectedTime);
            actual.ChargeOperation.Points[0].Price.Should().Be(100m);
            actual.ChargeOperation.Points[1].Position.Should().Be(2);
            actual.ChargeOperation.Points[1].Time.Should().Be(expectedTime);
            actual.ChargeOperation.Points[1].Price.Should().Be(200m);

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
            ChargeCommandConverter sut)
        {
            // Arrange
            var correlationId = Guid.NewGuid().ToString();
            var expectedTime = InstantPattern.ExtendedIso.Parse("2021-04-17T22:00:00Z").Value;
            using var reader = GetReaderAndArrangeTest(
                context,
                iso8601Durations,
                correlationId,
                expectedTime,
                "GreenEnergyHub.Charges.Tests.TestFiles.Valid_CIM_Charge_Without_Prices.xml");

            // Act
            var actualBundle = (ChargeCommandBundle)await sut.ConvertAsync(reader).ConfigureAwait(false);

            // Assert
            var actual = actualBundle.ChargeCommands.Single();

            // Charge operation
            actual.ChargeOperation.Id.Should().Be("36251479");
            actual.ChargeOperation.ChargeOwner.Should().Be("5799999925699");
            actual.ChargeOperation.Type.Should().Be(ChargeType.Fee);
            actual.ChargeOperation.ChargeId.Should().Be("888");
            actual.ChargeOperation.ChargeName.Should().Be("Test 888");
            actual.ChargeOperation.ChargeDescription.Should().Be("Description 888");
            actual.ChargeOperation.Resolution.Should().Be(Resolution.PT15M);
            actual.ChargeOperation.StartDateTime.Should().Be(expectedTime);
            actual.ChargeOperation.EndDateTime.Should().BeNull();
            actual.ChargeOperation.VatClassification.Should().Be(VatClassification.NoVat);
            actual.ChargeOperation.TransparentInvoicing.Should().BeFalse();
            actual.ChargeOperation.TaxIndicator.Should().BeFalse();

            // Prices, should not be any
            actual.ChargeOperation.Points.Should().BeEmpty();
            iso8601Durations.Verify(
                i => i.GetTimeFixedToDuration(
                    It.IsAny<Instant>(),
                    It.IsAny<string>(),
                    It.IsAny<int>()),
                Times.Never);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task ConvertAsync_WhenCalledWithValidCimMessageWithoutMasterData_ReturnsParsedObject(
            [Frozen] Mock<ICorrelationContext> context,
            [Frozen] Mock<IIso8601Durations> iso8601Durations,
            ChargeCommandConverter sut)
        {
            // Arrange
            var correlationId = Guid.NewGuid().ToString();
            var expectedTime = InstantPattern.ExtendedIso.Parse("2020-12-31T23:00:00Z").Value;
            using var reader = GetReaderAndArrangeTest(
                context,
                iso8601Durations,
                correlationId,
                expectedTime,
                "GreenEnergyHub.Charges.Tests.TestFiles.Valid_CIM_Charge_Prices_Without_Master_Data.xml");

            // Act
            var actualBundle = (ChargeCommandBundle)await sut.ConvertAsync(reader).ConfigureAwait(false);

            // Assert
            var actual = actualBundle.ChargeCommands.Single();

            // Charge operation, should only be partially filled
            actual.ChargeOperation.Id.Should().Be("36251480");
            actual.ChargeOperation.ChargeOwner.Should().Be("5799999925600");
            actual.ChargeOperation.Type.Should().Be(ChargeType.Subscription);
            actual.ChargeOperation.ChargeId.Should().Be("444");
            actual.ChargeOperation.ChargeName.Should().BeNullOrEmpty();
            actual.ChargeOperation.ChargeDescription.Should().BeNullOrEmpty();
            actual.ChargeOperation.Resolution.Should().Be(Resolution.P1M);
            actual.ChargeOperation.StartDateTime.Should().Be(expectedTime);
            actual.ChargeOperation.EndDateTime.Should().BeNull();
            actual.ChargeOperation.VatClassification.Should().Be(VatClassification.Unknown);
            actual.ChargeOperation.TransparentInvoicing.Should().BeFalse();
            actual.ChargeOperation.TaxIndicator.Should().BeFalse();

            // Points
            actual.ChargeOperation.Points.Should().HaveCount(2);
            actual.ChargeOperation.Points[0].Position.Should().Be(1);
            actual.ChargeOperation.Points[0].Time.Should().Be(expectedTime);
            actual.ChargeOperation.Points[0].Price.Should().Be(0.536m);
            actual.ChargeOperation.Points[1].Position.Should().Be(2);
            actual.ChargeOperation.Points[1].Time.Should().Be(expectedTime);
            actual.ChargeOperation.Points[1].Price.Should().Be(14.984m);

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
            ChargeCommandConverter sut)
        {
            // Arrange
            var correlationId = Guid.NewGuid().ToString();
            var expectedTime = InstantPattern.ExtendedIso.Parse("2022-10-31T23:00:00Z").Value;
            using var reader = GetReaderAndArrangeTest(
                context,
                iso8601Durations,
                correlationId,
                expectedTime,
                "GreenEnergyHub.Charges.Tests.TestFiles.CreateTariffsBundle.xml");

            // Act
            var actual = (ChargeCommandBundle)await sut.ConvertAsync(reader).ConfigureAwait(false);

            // Assert

            // Charge operation
            var actualFirstChargeCommand = actual.ChargeCommands.Single(x => x.ChargeOperation.Id == "36251480");
            actualFirstChargeCommand.ChargeOperation.ChargeOwner.Should().Be("8100000000030");
            actualFirstChargeCommand.ChargeOperation.Type.Should().Be(ChargeType.Tariff);
            actualFirstChargeCommand.ChargeOperation.ChargeId.Should().Be("ChId1234567890");
            actualFirstChargeCommand.ChargeOperation.ChargeName.Should().Be("Charge Tariff day Name 1");
            actualFirstChargeCommand.ChargeOperation.ChargeDescription.Should().Be("The charge description 1");
            actualFirstChargeCommand.ChargeOperation.Resolution.Should().Be(Resolution.P1D);
            actualFirstChargeCommand.ChargeOperation.StartDateTime.Should().Be(expectedTime);
            actualFirstChargeCommand.ChargeOperation.EndDateTime.Should().BeNull();
            actualFirstChargeCommand.ChargeOperation.VatClassification.Should().Be(VatClassification.NoVat);
            actualFirstChargeCommand.ChargeOperation.TransparentInvoicing.Should().BeFalse();
            actualFirstChargeCommand.ChargeOperation.TaxIndicator.Should().BeTrue();

            // Prices
            actualFirstChargeCommand.ChargeOperation.Points.Should().HaveCount(1);
            actualFirstChargeCommand.ChargeOperation.Points.First().Price.Should().Be(150.001m);

            var actualSecondChargeCommand = actual.ChargeCommands.Single(x => x.ChargeOperation.Id == "36251481");
            actualSecondChargeCommand.ChargeOperation.ChargeOwner.Should().Be("8100000000030");
            actualSecondChargeCommand.ChargeOperation.Type.Should().Be(ChargeType.Tariff);
            actualSecondChargeCommand.ChargeOperation.ChargeId.Should().Be("ChId1234567891");
            actualSecondChargeCommand.ChargeOperation.ChargeName.Should().Be("Charge Tariff day Name 2");
            actualSecondChargeCommand.ChargeOperation.ChargeDescription.Should().Be("The charge description 2");
            actualSecondChargeCommand.ChargeOperation.Resolution.Should().Be(Resolution.P1D);
            actualSecondChargeCommand.ChargeOperation.StartDateTime.Should().Be(expectedTime);
            actualSecondChargeCommand.ChargeOperation.EndDateTime.Should().BeNull();
            actualSecondChargeCommand.ChargeOperation.VatClassification.Should().Be(VatClassification.Vat25);
            actualSecondChargeCommand.ChargeOperation.TransparentInvoicing.Should().BeTrue();
            actualSecondChargeCommand.ChargeOperation.TaxIndicator.Should().BeFalse();

            // Prices
            actualSecondChargeCommand.ChargeOperation.Points.Should().HaveCount(1);
            actualSecondChargeCommand.ChargeOperation.Points.First().Price.Should().Be(200.001m);
        }

        private XmlReader GetReaderAndArrangeTest(
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
            return XmlReader.Create(stream, new XmlReaderSettings { Async = true });
        }

        private static Stream GetEmbeddedResource(string path)
        {
            var assembly = Assembly.GetExecutingAssembly();
            return EmbeddedStreamHelper.GetInputStream(assembly, path);
        }
    }
}
