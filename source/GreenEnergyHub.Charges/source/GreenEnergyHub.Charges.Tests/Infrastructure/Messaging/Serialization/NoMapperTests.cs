﻿// Copyright 2020 Energinet DataHub A/S
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

using System.Diagnostics.CodeAnalysis;
using GreenEnergyHub.Charges.Infrastructure.Messaging.Serialization;
using GreenEnergyHub.Messaging.Transport;
using GreenEnergyHub.TestHelpers;
using GreenEnergyHub.TestHelpers.Traits;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Utilities;
using Moq;
using Xunit;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Messaging.Serialization
{
    [Trait(TraitNames.Category, TraitValues.UnitTest)]
    public class NoMapperTests
    {
        [Theory]
        [InlineAutoDomainData]
        public void Convert_WhenCalled_ReturnsInput(
            [NotNull] Mock<IOutboundMessage> input,
            [NotNull] NoMapper sut)
        {
            // Act
            var result = sut.Convert(input.Object);

            // Assert
            Assert.Equal(result, input.Object);
        }
    }
}
