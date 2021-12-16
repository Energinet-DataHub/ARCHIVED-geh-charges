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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Infrastructure.CimDeserialization.ChargeLinkBundle;
using GreenEnergyHub.Charges.Infrastructure.Core.Correlation;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Moq;
using NodaTime.Text;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.ChargeLinkBundle.Cim
{
    [UnitTest]
    public class ChargeLinkCommandConverterTests
    {
        [Theory]
        [InlineAutoMoqData]
        public async Task ConvertAsync_WhenCalledWithValidCimMessage_ReturnsParsedObject(
            [Frozen] Mock<ICorrelationContext> context,
            ChargeLinkCommandConverter sut)
        {
            // Arrange
            var correlationId = Guid.NewGuid().ToString();
            context.Setup(c => c.Id).Returns(correlationId);

            var stream = GetEmbeddedResource("GreenEnergyHub.Charges.Tests.TestFiles.Valid_CIM_ChargeLink.xml");
            using var reader = XmlReader.Create(stream, new XmlReaderSettings { Async = true });

            // Act
            var result = (ChargeLinksCommand)await sut.ConvertAsync(reader).ConfigureAwait(false);

            // Assert
            var chargeLink = result.ChargeLinks.First();
            Assert.Equal("578032999778756222", result.MeteringPointId);

            // Document
            Assert.Equal("DocId_Valid_001", result.Document.Id);
            Assert.Equal(DocumentType.RequestChangeBillingMasterData, result.Document.Type);
            Assert.Equal(BusinessReasonCode.UpdateMasterDataSettlement, result.Document.BusinessReasonCode);
            Assert.Equal("8100000000016", result.Document.Sender.Id);
            Assert.Equal(MarketParticipantRole.GridAccessProvider, result.Document.Sender.BusinessProcessRole);
            Assert.Equal("5790001330552", result.Document.Recipient.Id);
            Assert.Equal(MarketParticipantRole.MeteringPointAdministrator, result.Document.Recipient.BusinessProcessRole);
            Assert.Equal(InstantPattern.ExtendedIso.Parse("2021-07-05T13:20:02.387Z").Value, result.Document.CreatedDateTime);

            // ChargeLink
            Assert.Equal("rId_Valid_001", chargeLink.OperationId);
            Assert.Equal(InstantPattern.ExtendedIso.Parse("2021-09-27T22:00:00Z").Value, chargeLink.StartDateTime);
            Assert.Equal(InstantPattern.ExtendedIso.Parse("2021-11-05T23:00:00Z").Value, chargeLink.EndDateTime);
            Assert.Equal("ChargeId01", chargeLink.SenderProvidedChargeId);
            Assert.Equal(1, chargeLink.Factor);
            Assert.Equal("8100000000016", chargeLink.ChargeOwnerId);
            Assert.Equal(ChargeType.Tariff, chargeLink.ChargeType);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task ConvertAsync_WhenCalledWithValidCimMessageContainingUnusedCimContent_ReturnsParsedObject(
            [Frozen] Mock<ICorrelationContext> context,
            ChargeLinkCommandConverter sut)
        {
            // Arrange
            var correlationId = Guid.NewGuid().ToString();
            context.Setup(c => c.Id).Returns(correlationId);

            var stream = GetEmbeddedResource("GreenEnergyHub.Charges.Tests.TestFiles.Valid_CIM_ChargeLink_WithUnusedCimContent.xml");
            using var reader = XmlReader.Create(stream, new XmlReaderSettings { Async = true });

            // Act
            var result = (ChargeLinksCommand)await sut.ConvertAsync(reader).ConfigureAwait(false);

            // Assert
            Assert.Equal("DocId_Valid_002", result.Document.Id);
            Assert.Equal("rId_Valid_002", result.ChargeLinks.First().OperationId);

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task ConvertAsync_WhenCalledWithValidCimMessageWithoutEndDate_ReturnsParsedObject(
            [Frozen] Mock<ICorrelationContext> context,
            ChargeLinkCommandConverter sut)
        {
            // Arrange
            var correlationId = Guid.NewGuid().ToString();
            context.Setup(c => c.Id).Returns(correlationId);

            var stream = GetEmbeddedResource("GreenEnergyHub.Charges.Tests.TestFiles.Valid_CIM_ChargeLink_WithoutEndDate.xml");
            using var reader = XmlReader.Create(stream, new XmlReaderSettings { Async = true });

            // Act
            var result = (ChargeLinksCommand)await sut.ConvertAsync(reader).ConfigureAwait(false);

            // Assert
            Assert.Equal("DocId_Valid_003", result.Document.Id);
            Assert.Equal("rId_Valid_003", result.ChargeLinks.First().OperationId);
            Assert.Null(result.ChargeLinks.First().EndDateTime);

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task ConvertAsync_WhenCalledWithValidBundleChargeLinks_ReturnsParsedObjects(
            [Frozen] Mock<ICorrelationContext> context,
            ChargeLinkCommandConverter sut)
        {
            // Arrange
            var correlationId = Guid.NewGuid().ToString();
            context.Setup(c => c.Id).Returns(correlationId);

            var stream = GetEmbeddedResource("GreenEnergyHub.Charges.Tests.TestFiles.Valid_CIM_ChargeLink_Bundle.xml");
            using var reader = XmlReader.Create(stream, new XmlReaderSettings { Async = true });

            // Act
            var result = (ChargeLinksCommandBundle)await sut.ConvertAsync(reader).ConfigureAwait(false);

            // Assert
            // Document
            var document =
                result.ChargeLinksCommands.First().Document;
            document.Id.Should().Be("DocId_Valid_001");
            document.Type.Should().Be(DocumentType.RequestChangeBillingMasterData);
            document.BusinessReasonCode.Should().Be(BusinessReasonCode.UpdateMasterDataSettlement);
            document.Sender.Id.Should().Be("8100000000016");
            document.Sender.BusinessProcessRole.Should().Be(MarketParticipantRole.GridAccessProvider);
            document.Recipient.Id.Should().Be("5790001330552");
            document.Recipient.BusinessProcessRole.Should().Be(MarketParticipantRole.MeteringPointAdministrator);
            document.CreatedDateTime.Should().Be(InstantPattern.ExtendedIso.Parse("2021-07-05T13:20:02.387Z").Value);

            // ChargeLink 1
            var chargeLink1 =
                result.ChargeLinksCommands.Single(x => x.ChargeLinks.First().OperationId == "rId_Valid_001").ChargeLinks
                    .First();
            chargeLink1.StartDateTime.Should().Be(InstantPattern.ExtendedIso.Parse("2021-09-27T22:00:00Z").Value);
            chargeLink1.EndDateTime.Should().Be(InstantPattern.ExtendedIso.Parse("2021-11-05T23:00:00Z").Value);
            chargeLink1.SenderProvidedChargeId.Should().Be("ChargeId01");
            chargeLink1.Factor.Should().Be(1);
            chargeLink1.ChargeOwnerId.Should().Be("8100000000016");
            chargeLink1.ChargeType.Should().Be(ChargeType.Tariff);

            // ChargeLink 2
            var chargeLink2 =
                result.ChargeLinksCommands.Single(x => x.ChargeLinks.First().OperationId == "rId_Valid_002").ChargeLinks
                    .First();
            chargeLink2.StartDateTime.Should().Be(InstantPattern.ExtendedIso.Parse("2021-11-27T22:00:00Z").Value);
            chargeLink2.EndDateTime.Should().Be(InstantPattern.ExtendedIso.Parse("2021-12-05T23:00:00Z").Value);
            chargeLink2.SenderProvidedChargeId.Should().Be("ChargeId01");
            chargeLink2.Factor.Should().Be(1);
            chargeLink2.ChargeOwnerId.Should().Be("8100000000016");
            chargeLink2.ChargeType.Should().Be(ChargeType.Tariff);
        }

        private static Stream GetEmbeddedResource(string path)
        {
            var assembly = Assembly.GetExecutingAssembly();
            return EmbeddedStreamHelper.GetInputStream(assembly, path);
        }
    }
}
