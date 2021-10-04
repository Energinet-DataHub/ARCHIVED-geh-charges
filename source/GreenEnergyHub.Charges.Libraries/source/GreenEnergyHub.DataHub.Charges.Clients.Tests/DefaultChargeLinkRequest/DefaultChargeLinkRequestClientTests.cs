using System.Threading.Tasks;
using GreenEnergyHub.DataHub.Charges.Libraries.DefaultChargeLinkRequest;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.DataHub.Charges.Clients.CreateDefaultChargeLink.Tests.DefaultChargeLinkRequest
{
    public class DefaultChargeLinkRequestClientTests
    {
        [UnitTest]
        [Fact]
        public async Task WhenCalled()
        {
            // Arrange
            await using var sut = new DefaultChargeLinkRequestClient("connectionString", "respondQueueName");

            // Act
            await sut.CreateDefaultChargeLinksRequestAsync("meteringPointId", "FAC554D4-B434-414A-870A-5026644D8A76").ConfigureAwait(false);

            // Assert
            sut.
        }
    }
}
