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

// The following can be used again on the other side of the first event publish PR
/*using System;
using System.Collections.Generic;
using DbUp.Engine;
using GreenEnergyHub.Charges.ApplyDBMigrationsApp.Helpers;
using Moq;
using Xunit;

namespace GreenEnergyHub.Charges.Tests.ApplyDBMigrationsApp.Helpers
{
    [Trait("Category", "Unit")]
    public class ResultReporterTests
    {
        private const int SuccessResult = 0;
        private const int FailureResult = -1;

        [Theory]
        [AutoDomainData]
        internal void ReportResult_WhenSuccess_ReturnSuccessfulResponse(
            Mock<IEnumerable<SqlScript>> scripts,
            Mock<SqlScript> script,
            Mock<Exception> exception)
        {
            // Arrange
            var upgradeResult = new DatabaseUpgradeResult(scripts.Object, true, exception.Object, script.Object);

            // Act
            var result = ResultReporter.ReportResult(upgradeResult);

            // Assert
            Assert.Equal(SuccessResult, result);
        }

        [Theory]
        [AutoDomainData]
        internal void ReportResult_WhenFailure_ReturnFailureResponse(
            Mock<IEnumerable<SqlScript>> scripts,
            Mock<SqlScript> script,
            Mock<Exception> exception)
        {
            // Arrange
            var upgradeResult = new DatabaseUpgradeResult(scripts.Object, false, exception.Object, script.Object);

            // Act
            var result = ResultReporter.ReportResult(upgradeResult);

            // Assert
            Assert.Equal(FailureResult, result);
        }
    }
}*/
