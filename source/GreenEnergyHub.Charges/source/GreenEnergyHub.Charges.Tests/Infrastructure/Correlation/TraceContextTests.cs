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

using FluentAssertions;
using GreenEnergyHub.Charges.Infrastructure.Core.Correlation;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Correlation
{
    [UnitTest]
    public class TraceContextTests
    {
        [Theory]
        [InlineData("", false)]
        [InlineData("00,0af7651916cd43dd8448eb211c80319c,b9c7c989f97918e1,00", false)]
        [InlineData("00-0af7651916cd43dd8448eb211c80319c-b9c7c989f97918e1-00", true)]
        public void TraceContextShouldParse(string traceContextString, bool validated)
        {
            var traceContext = TraceContext.Parse(traceContextString);

            traceContext.IsValid.Should().Be(validated);
        }
    }
}
