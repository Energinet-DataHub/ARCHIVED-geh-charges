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

using System;
using System.Threading.Tasks;
using FluentAssertions;
using GreenEnergyHub.Charges.Application.ChargeLinks.Handlers;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.ChargeLinks.Handlers
{
    [UnitTest]
    public class ChargeLinkDataAvailableNotifierAndReplyHandlerTests
    {
        [Theory]
        [InlineAutoMoqData]
        public async Task NotifyAsync_WhenEventIsNull_ThrowsArgumentNullException(
            ChargeLinkDataAvailableNotifierAndReplyHandler sut)
        {
            await sut
                .Invoking(notifier => notifier.NotifyAndReplyAsync(null!))
                .Should()
                .ThrowExactlyAsync<ArgumentNullException>();
        }
    }
}
