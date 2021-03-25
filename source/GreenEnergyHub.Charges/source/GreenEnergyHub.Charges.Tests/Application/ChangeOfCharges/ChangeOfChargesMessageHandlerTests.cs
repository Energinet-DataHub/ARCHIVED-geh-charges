using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using GreenEnergyHub.Charges.Application.ChangeOfCharges;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using GreenEnergyHub.Charges.Tests.Builders;
using GreenEnergyHub.TestHelpers;
using GreenEnergyHub.TestHelpers.Traits;
using Moq;
using Xunit;

namespace GreenEnergyHub.Charges.Tests.Application.ChangeOfCharges
{
    [Trait(TraitNames.Category, TraitValues.UnitTest)]
    public class ChangeOfChargesMessageHandlerTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task HandleAsync_WhenCalledWithMultipleTransactions_ShouldCallMultiepleTimes(
            [NotNull] [Frozen] Mock<IChangeOfChargesTransactionHandler> changeOfChargesTransactionHandler,
            [NotNull] ChangeOfChargesMessageHandler sut)
        {
            // Arrange
            var transactionBuilder = new ChangeOfChargesTransactionBuilder();
            var changeOfChargesMessage = new ChangeOfChargesMessageBuilder()
                .WithTransaction(transactionBuilder.Build())
                .WithTransaction(transactionBuilder.Build())
                .WithTransaction(transactionBuilder.Build())
                .Build();

            // Act
            await sut.HandleAsync(changeOfChargesMessage).ConfigureAwait(false);

            // Assert
            changeOfChargesTransactionHandler
                .Verify(v => v.HandleAsync(It.IsAny<ChangeOfChargesTransaction>()), Times.Exactly(3));
        }
    }
}
