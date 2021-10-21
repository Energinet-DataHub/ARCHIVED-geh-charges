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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using AutoFixture.Xunit2;
using GreenEnergyHub.Charges.Domain.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Infrastructure.ChargeBundle.Cim;
using GreenEnergyHub.Charges.Infrastructure.Correlation;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Moq;
using NodaTime.Text;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.ChargeBundle.Cim
{
    [UnitTest]
    public class ChargeCommandConverterTests
    {
        [Theory]
        [InlineAutoMoqData]
        public async Task ConvertAsync_WhenCalledWithValidCimMessage_ReturnsParsedObject(
            [NotNull][Frozen] Mock<ICorrelationContext> context,
            [NotNull] ChargeCommandConverter sut)
        {
            // Arrange
            var correlationId = Guid.NewGuid().ToString();
            context.Setup(c => c.Id).Returns(correlationId);

            var stream = GetEmbeddedResource("GreenEnergyHub.Charges.Tests.TestFiles.Valid_CIM_Charge.xml");
            using var reader = XmlReader.Create(stream, new XmlReaderSettings { Async = true });

            // Act
            var result = (ChargeCommand)await sut.ConvertAsync(reader).ConfigureAwait(false);

            // Assert
            Assert.Equal(correlationId, result.CorrelationId);

            // Document
            Assert.Equal("25369874", result.Document.Id);
            Assert.Equal(DocumentType.RequestUpdateChargeInformation, result.Document.Type);
            Assert.Equal(BusinessReasonCode.UpdateChargeInformation, result.Document.BusinessReasonCode);
            Assert.Equal("5799999925698", result.Document.Sender.Id);
            Assert.Equal(MarketParticipantRole.GridAccessProvider, result.Document.Sender.BusinessProcessRole);
            Assert.Equal("5790001330552", result.Document.Recipient.Id);
            Assert.Equal(MarketParticipantRole.MeteringPointAdministrator, result.Document.Recipient.BusinessProcessRole);
            Assert.Equal(InstantPattern.ExtendedIso.Parse("2021-12-17T09:30:47Z").Value, result.Document.CreatedDateTime);

            // Charge operation
            Assert.Equal("36251478", result.ChargeOperation.Id);
            Assert.Equal("5799999925698", result.ChargeOperation.ChargeOwner);
            Assert.Equal(ChargeType.Tariff, result.ChargeOperation.Type);
            Assert.Equal("253C", result.ChargeOperation.ChargeId);
            Assert.Equal("Elafgift 2019", result.ChargeOperation.ChargeName);
            Assert.Equal("Dette er elafgiftssatsten for 2019", result.ChargeOperation.ChargeDescription);
            Assert.Equal(Resolution.PT1H, result.ChargeOperation.Resolution);
            /*// ChargeLink
            Assert.Equal("rId_Valid_001", result.ChargeLink.OperationId);
            Assert.Equal("578032999778756222", result.ChargeLink.MeteringPointId);
            Assert.Equal(InstantPattern.ExtendedIso.Parse("2021-09-27T22:00:00Z").Value, result.ChargeLink.StartDateTime);
            Assert.Equal(InstantPattern.ExtendedIso.Parse("2021-11-05T23:00:00Z").Value, result.ChargeLink.EndDateTime);
            Assert.Equal("ChargeId01", result.ChargeLink.SenderProvidedChargeId);
            Assert.Equal(1, result.ChargeLink.Factor);
            Assert.Equal("8100000000016", result.ChargeLink.ChargeOwner);
            Assert.Equal(ChargeType.Tariff, result.ChargeLink.ChargeType);*/
        }

/*        [Theory]
        [InlineAutoMoqData]
        public async Task ConvertAsync_WhenCalledWithValidCimMessageContainingUnusedCimContent_ReturnsParsedObject(
            [NotNull][Frozen] Mock<ICorrelationContext> context,
            [NotNull] ChargeLinkCommandConverter sut)
        {
            // Arrange
            var correlationId = Guid.NewGuid().ToString();
            context.Setup(c => c.Id).Returns(correlationId);

            var stream = GetEmbeddedResource("GreenEnergyHub.Charges.Tests.TestFiles.Valid_CIM_ChargeLink_WithUnusedCimContent.xml");
            using var reader = XmlReader.Create(stream, new XmlReaderSettings { Async = true });

            // Act
            var result = (ChargeLinkCommand)await sut.ConvertAsync(reader).ConfigureAwait(false);

            // Assert
            Assert.Equal("DocId_Valid_002", result.Document.Id);
            Assert.Equal("rId_Valid_002", result.ChargeLink.OperationId);

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task ConvertAsync_WhenCalledWithValidCimMessageWithoutEndDate_ReturnsParsedObject(
            [NotNull][Frozen] Mock<ICorrelationContext> context,
            [NotNull] ChargeLinkCommandConverter sut)
        {
            // Arrange
            var correlationId = Guid.NewGuid().ToString();
            context.Setup(c => c.Id).Returns(correlationId);

            var stream = GetEmbeddedResource("GreenEnergyHub.Charges.Tests.TestFiles.Valid_CIM_ChargeLink_WithoutEndDate.xml");
            using var reader = XmlReader.Create(stream, new XmlReaderSettings { Async = true });

            // Act
            var result = (ChargeLinkCommand)await sut.ConvertAsync(reader).ConfigureAwait(false);

            // Assert
            Assert.Equal("DocId_Valid_003", result.Document.Id);
            Assert.Equal("rId_Valid_003", result.ChargeLink.OperationId);
            Assert.Null(result.ChargeLink.EndDateTime);

            await Task.CompletedTask.ConfigureAwait(false);
        }
*/
        private static async Task<byte[]> GetEmbeddedResourceAsBytes(string path)
        {
            var input = GetEmbeddedResource(path);

            var byteInput = new byte[input.Length];
            await input.ReadAsync(byteInput.AsMemory(0, (int)input.Length)).ConfigureAwait(false);
            return byteInput;
        }

        private static Stream GetEmbeddedResource(string path)
        {
            var assembly = Assembly.GetExecutingAssembly();
            return EmbeddedStreamHelper.GetInputStream(assembly, path);
        }
    }
}
