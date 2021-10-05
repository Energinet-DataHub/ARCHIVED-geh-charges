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
using System.Diagnostics;
using System.Threading;

namespace GreenEnergyHub.FunctionApp.TestCommon.FunctionAppHost
{
    /// <summary>
    /// Can be used to wait for a Host to start within a process,
    /// by listening on all Output and expect a specific message to appear
    /// when the host is ready.
    /// </summary>
    public class HostStartedOutputListener
    {
        public HostStartedOutputListener(Process process, Func<DataReceivedEventArgs, bool> isStartedEventPredicate)
        {
            Process = process ?? throw new ArgumentNullException(nameof(process));
            IsStartedEventPredicate = isStartedEventPredicate ?? throw new ArgumentNullException(nameof(isStartedEventPredicate));

            if (!Process.StartInfo.RedirectStandardOutput)
            {
                throw new InvalidOperationException($"Process must have '{nameof(Process.StartInfo.RedirectStandardOutput)}' enabled.");
            }

            StartedWaitHandle = new ManualResetEventSlim(false);
            WaitForStartedEvent = true;

            Process.OutputDataReceived += OnListenForStartedEvent;
        }

        private Process Process { get; }

        private Func<DataReceivedEventArgs, bool> IsStartedEventPredicate { get; }

        private ManualResetEventSlim StartedWaitHandle { get; }

        /// <summary>
        /// Ensure we don't have a race condition when unregistering <see cref="OnListenForStartedEvent"/>
        /// and disposing <see cref="StartedWaitHandle "/> (e.g. which would cause object disposed exception).
        /// </summary>
        private bool WaitForStartedEvent { get; set; }

        /// <summary>
        /// Validate if Host is started and ready within timeout.
        /// </summary>
        /// <param name="timeout">Represents the max. time to wait for the Host to be ready.</param>
        /// <returns>True if the Host is started and ready to serve; otherwise, false.</returns>
        public bool WaitForStarted(TimeSpan timeout)
        {
            var isHostStarted = StartedWaitHandle.Wait(timeout);
            WaitForStartedEvent = false;

            // Cleanup
            Process.OutputDataReceived -= OnListenForStartedEvent;
            StartedWaitHandle.Dispose();

            return isHostStarted;
        }

        private void OnListenForStartedEvent(object sender, DataReceivedEventArgs outputEvent)
        {
            if (outputEvent.Data != null
                && WaitForStartedEvent
                && IsStartedEventPredicate(outputEvent))
            {
                StartedWaitHandle.Set();
            }
        }
    }
}
