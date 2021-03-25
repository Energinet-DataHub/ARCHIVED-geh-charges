using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using GreenEnergyHub.Charges.Application.ChangeOfCharges;
using GreenEnergyHub.Charges.Domain.Events.Local;
using GreenEnergyHub.Charges.Tests.Builders;
using GreenEnergyHub.TestHelpers;
using GreenEnergyHub.TestHelpers.Traits;
using Moq;
using Xunit;

namespace GreenEnergyHub.Charges.Tests.Application.ChangeOfCharges
{
    [Trait(TraitNames.Category, TraitValues.UnitTest)]
    public class ChangeOfChargesTransactionHandlerTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task ChangeOfChargesTransactionHandler_WhenCalled_ShouldCallPublisher(
            [NotNull] [Frozen] Mock<ILocalEventPublisher> localEventPublisher,
            [NotNull] ChangeOfChargesTransactionHandler sut)
        {
            // Arrange
            var transaction = new ChangeOfChargesTransactionBuilder().Build();

            // Act
            await sut.HandleAsync(transaction).ConfigureAwait(false);

            // Assert
            localEventPublisher.Verify(x => x.PublishAsync(It.Is<ChargeTransactionReceived>(localEvent =>
                localEvent.Transaction == transaction && localEvent.CorrelationId == transaction.CorrelationId)));
        }
    }
}
