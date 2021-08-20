using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using GreenEnergyHub.Charges.Application;
using GreenEnergyHub.Charges.Domain.Events.Integration;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.TestHelpers;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.MeteringPoints
{
    [UnitTest]
    public class MeteringPointCreatedHandlerTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task HandleAsync_WhenCalled_ShouldCallRepository(
            [NotNull][Frozen] Mock<IMeteringPointCreatedHandler> meteringPointCreatedHandler,
            [NotNull] MeteringPointCreatedEvent meteringPointCreatedEvent,
            [NotNull] MeteringPointCreatedHandler sut)
        {
            // Act
            await sut.HandleAsync(meteringPointCreatedEvent).ConfigureAwait(false);

            // Assert
            meteringPointCreatedHandler
                .Verify(v => v.HandleAsync(It.IsAny<MeteringPointCreatedEvent>()), Times.Exactly(3));
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_IfEventIsNull_ThrowsArgumentNullException(
            [NotNull] MeteringPointCreatedHandler sut)
        {
            // Arrange
            MeteringPointCreatedEvent? meteringPointCreatedEvent = null;

            // Act / Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                    () => sut.HandleAsync(meteringPointCreatedEvent!))
                .ConfigureAwait(false);
        }
    }
}
