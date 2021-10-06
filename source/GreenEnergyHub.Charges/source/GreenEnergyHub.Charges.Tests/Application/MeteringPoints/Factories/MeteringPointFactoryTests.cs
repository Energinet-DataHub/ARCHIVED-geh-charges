using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Domain.MeteringPointCreatedEvents;
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using GreenEnergyHub.TestHelpers;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.MeteringPoints.Factories
{
    [UnitTest]
    public class MeteringPointFactoryTests
    {
        [Fact]
        public void Create_WhenCalledWithNull_ThrowsException()
        {
            ConsumptionMeteringPointCreatedEvent? input = null;
            Assert.Throws<ArgumentNullException>(() => MeteringPointFactory.Create(input!));
        }
    }
}
