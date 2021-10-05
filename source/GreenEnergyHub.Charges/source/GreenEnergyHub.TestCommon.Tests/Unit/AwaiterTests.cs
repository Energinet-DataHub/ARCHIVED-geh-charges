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
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Xunit.Categories;
using Xunit.Sdk;

namespace GreenEnergyHub.TestCommon.Tests.Unit
{
    public class AwaiterTests
    {
        [UnitTest]
        public class WaitUntilConditionAsync
        {
            [Fact]
            public async Task When_FuncConditionIsMet_Then_MethodReturn()
            {
                // Arrange
                var condition = 0;

                // Act
                Func<Task> act = () => Awaiter.WaitUntilConditionAsync(() => condition++ == 1, TimeSpan.FromMilliseconds(200), TimeSpan.FromMilliseconds(50));

                // Assert
                await act.Should().NotThrowAsync();
            }

            [Fact]
            public async Task When_FuncConditionIsNotMetWithinMaxWait_Then_ExceptionIsThrown()
            {
                // Arrange

                // Act
                Func<Task> act = () => Awaiter.WaitUntilConditionAsync(() => false, TimeSpan.FromMilliseconds(200));

                // Assert
                await act.Should().ThrowAsync<XunitException>();
            }

            [Fact]
            public async Task When_FuncTaskConditionIsMet_Then_MethodReturn()
            {
                // Arrange
                var condition = 0;

                // Act
                Func<Task> act = () => Awaiter.WaitUntilConditionAsync(() => Task.FromResult(condition++ == 1), TimeSpan.FromMilliseconds(200));

                // Assert
                await act.Should().NotThrowAsync();
            }

            [Fact]
            public async Task When_FuncTaskConditionIsNotMetWithinMaxWait_Then_ExceptionIsThrown()
            {
                // Arrange

                // Act
                Func<Task> act = () => Awaiter.WaitUntilConditionAsync(() => Task.FromResult(false), TimeSpan.FromMilliseconds(200));

                // Assert
                await act.Should().ThrowAsync<XunitException>();
            }
        }

        [UnitTest]
        public class TryWaitUntilConditionAsync
        {
            [Fact]
            public async Task When_FuncConditionIsMet_Then_MethodReturnTrue()
            {
                // Arrange
                var condition = 0;

                // Act
                var result = await Awaiter.TryWaitUntilConditionAsync(() => condition++ == 1, TimeSpan.FromMilliseconds(200));

                // Assert
                result.Should().BeTrue();
            }

            [Fact]
            public async Task When_FuncConditionIsNotMetWithinMaxWait_Then_MethodReturnFalse()
            {
                // Arrange

                // Act
                var result = await Awaiter.TryWaitUntilConditionAsync(() => false, TimeSpan.FromMilliseconds(200), TimeSpan.FromMilliseconds(50));

                // Assert
                result.Should().BeFalse();
            }

            [Fact]
            public async Task When_FuncTaskConditionIsMet_Then_MethodReturnTrue()
            {
                // Arrange
                var condition = 0;

                // Act
                var result = await Awaiter.TryWaitUntilConditionAsync(() => Task.FromResult(condition++ == 1), TimeSpan.FromMilliseconds(200));

                // Assert
                result.Should().BeTrue();
            }

            [Fact]
            public async Task When_FuncTaskConditionIsNotMetWithinMaxWait_Then_MethodReturnTrue()
            {
                // Arrange

                // Act
                var result = await Awaiter.TryWaitUntilConditionAsync(() => Task.FromResult(false), TimeSpan.FromMilliseconds(200));

                // Assert
                result.Should().BeFalse();
            }
        }
    }
}
