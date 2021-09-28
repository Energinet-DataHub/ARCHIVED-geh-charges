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

using Xunit.Abstractions;

namespace GreenEnergyHub.TestCommon.Diagnostics
{
    /// <summary>
    /// Can be used within common test classes to write diagnostics information to output:
    /// * Running test in debug mode from Visual Studio Test Explorer, will display the output
    /// in the "Output" window under "Debug" source. To avoid breaking on exceptions disable
    /// all "Common Language Runtime Exceptions" in "Exception Settings" in Visual Studio.
    /// * Running test on the Build Agent, will display the output in the build output (console).
    /// * Running test locally using 'dotnet test --logger "console;verbosity=detailed"' will
    /// display output that is written while the <see cref="TestOutputHelper"/> is set (e.g. since it
    /// is only set during active tests, it will not display the host startup log, as this is written
    /// during fixture initialization).
    ///
    /// We would have liked to use xUnit "IMessageSink". However, when using SpecFlow, we could not
    /// get a reference to it.
    /// </summary>
    public interface ITestDiagnosticsLogger
    {
        /// <summary>
        /// For attaching a xUnit <see cref="ITestOutputHelper"/> to be able to write output for the
        /// active test.
        /// </summary>
        ITestOutputHelper? TestOutputHelper { get; set; }

        /// <summary>
        /// Writes a message to output.
        /// </summary>
        /// <param name="message">A message to write.</param>
        void WriteLine(string message);
    }
}
