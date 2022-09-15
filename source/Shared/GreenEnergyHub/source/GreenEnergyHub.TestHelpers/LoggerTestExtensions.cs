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
using Microsoft.Extensions.Logging;
using Moq;

namespace GreenEnergyHub.TestHelpers
{
    public static class LoggerTestExtensions
    {
        /// <summary>
        /// Taken from https://github.com/WestDiscGolf/Random/blob/master/LoggerUnitTests/test/LoggerUnitTests/LoggerTestExtensions.cs
        /// </summary>
        public static Mock<ILogger<T>> VerifyLoggerWasCalled<T>(this Mock<ILogger<T>> logger, string expectedMessage, LogLevel logLevel)
        {
            Func<object, Type, bool> stringComparer = (v, _) => string.Compare(v.ToString(), expectedMessage, StringComparison.InvariantCulture) == 0;

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            logger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == logLevel),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => stringComparer(v, t)),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));

            return logger;
        }

        public static Mock<ILogger> VerifyLoggerWasCalled(this Mock<ILogger> logger, string expectedMessage, LogLevel logLevel)
        {
            Func<object, Type, bool> stringComparer = (v, _) => string.Compare(v.ToString(), expectedMessage, StringComparison.InvariantCulture) == 0;

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            logger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == logLevel),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => stringComparer(v, t)),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));

            return logger;
        }
    }
}
