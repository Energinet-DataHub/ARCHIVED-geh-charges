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
using Xunit.Sdk;

namespace GreenEnergyHub.TestCommon
{
    // TODO: Would rather have something like the following: https://stackoverflow.com/questions/29089417/c-sharp-wait-until-condition-is-true
    public static class Awaiter
    {
        /// <summary>
        /// Completes when the "condition" returns true or
        /// when the time limit is exceeded and a XUnitException is thrown.
        /// </summary>
        /// <param name="condition">Boolean condition that is checked.</param>
        /// <param name="timeLimit">Time limit until exception is thrown</param>
        /// <param name="delay">Delay between each check on the condition</param>
        public static async Task WaitUntilConditionAsync(Func<bool> condition, TimeSpan timeLimit, TimeSpan? delay = null)
        {
            if (!await TryWaitUntilConditionAsync(condition, timeLimit, delay))
            {
                throw new XunitException("Condition not reached before time limit.");
            }
        }

        /// <summary>
        /// Completes when the "condition" returns true or
        /// when the time limit is exceeded and a XUnitException is thrown.
        /// </summary>
        /// <param name="condition">Boolean condition that is checked.</param>
        /// <param name="timeLimit">Time limit until exception is thrown</param>
        /// <param name="delay">Delay between each check on the condition</param>
        public static async Task WaitUntilConditionAsync(Func<Task<bool>> condition, TimeSpan timeLimit, TimeSpan? delay = null)
        {
            if (!await TryWaitUntilConditionAsync(condition, timeLimit, delay))
            {
                throw new XunitException("Condition not reached before time limit.");
            }
        }

        /// <summary>
        /// Completes when the "condition" returns true or
        /// when the time limit is exceeded and then false is returned.
        /// </summary>
        /// <param name="condition">Boolean condition that is checked.</param>
        /// <param name="timeLimit">Time limit until false is returned</param>
        /// <param name="delay">Delay between each check on the condition</param>
        /// <returns>True when condition resolves to true and false when the time limit is exceeded</returns>
        public static async Task<bool> TryWaitUntilConditionAsync(Func<bool> condition, TimeSpan timeLimit, TimeSpan? delay = null)
        {
            var startTime = Environment.TickCount;
            while (!condition())
            {
                if (!await CheckMaxWaitAndDelayAsync(timeLimit.TotalMilliseconds, startTime, delay).ConfigureAwait(false))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Completes when the "condition" returns true or
        /// when the time limit is exceeded and then false is returned.
        /// </summary>
        /// <param name="condition">Boolean condition that is checked.</param>
        /// <param name="timeLimit">Time limit until false is returned</param>
        /// <param name="delay">Delay between each check on the condition</param>
        /// <returns>True when condition resolves to true and false when the time limit is exceeded</returns>
        public static async Task<bool> TryWaitUntilConditionAsync(Func<Task<bool>> condition, TimeSpan timeLimit, TimeSpan? delay = null)
        {
            var startTime = Environment.TickCount;
            while (!await condition().ConfigureAwait(false))
            {
                if (!await CheckMaxWaitAndDelayAsync(timeLimit.TotalMilliseconds, startTime, delay).ConfigureAwait(false))
                {
                    return false;
                }
            }

            return true;
        }

        private static async Task<bool> CheckMaxWaitAndDelayAsync(double maxWaitMilliseconds, int startTime, TimeSpan? delay = null)
        {
            if (maxWaitMilliseconds >= Environment.TickCount - startTime)
            {
                delay ??= TimeSpan.FromMilliseconds(100);
                await Task.Delay(delay.Value).ConfigureAwait(false);
                return true;
            }

            return false;
        }
    }
}
