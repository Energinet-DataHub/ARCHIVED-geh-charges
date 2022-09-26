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

using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Energinet.DataHub.Core.SchemaValidation;
using GreenEnergyHub.Charges.Infrastructure.CimDeserialization.ChargeBundle;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.CimDeserialization.ChargeBundle
{
    [UnitTest]
    public class ChargeCommandDeserializerTests
    {
        [Theory]
        [InlineAutoMoqData]
        public async Task FromBytesAsync_WhenConvertThrowsException_ShouldRethrow(
            [Frozen] Mock<IChargeCommandBundleConverter> chargeCommandConverter,
            ChargeCommandDeserializer sut)
        {
            // Arrange
            using var stream = new MemoryStream();
            ContentStreamHelper.GetFileAsStream(stream, "TestFiles/Syntax_Valid_CIM_Charge.xml");

            var byteArray = await GetBytesFromStreamAsync(stream);
            chargeCommandConverter.Setup(x => x.ConvertAsync(It.IsAny<SchemaValidatingReader>()))
                .ThrowsAsync(new NoChargeOperationFoundException());

            // Assert
            await Assert.ThrowsAsync<NoChargeOperationFoundException>(async () => await sut.FromBytesAsync(byteArray));
        }

        private static async Task<byte[]> GetBytesFromStreamAsync(Stream data)
        {
            await using var stream = new MemoryStream();
            await data.CopyToAsync(stream, CancellationToken.None).ConfigureAwait(false);
            var bytes = stream.ToArray();
            return bytes;
        }
    }
}
