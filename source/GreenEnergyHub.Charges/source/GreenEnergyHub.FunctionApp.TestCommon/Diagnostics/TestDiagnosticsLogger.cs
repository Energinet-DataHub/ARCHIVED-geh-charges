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
using Xunit.Abstractions;

namespace GreenEnergyHub.FunctionApp.TestCommon.Diagnostics
{
    /// <inheritdoc cref="ITestDiagnosticsLogger"/>
    public class TestDiagnosticsLogger : ITestDiagnosticsLogger
    {
        public ITestOutputHelper? TestOutputHelper { get; set; }

        public void WriteLine(string message)
        {
            // Running test in debug mode from Visual Studio Test Explorer, will display the output
            // in the "Output" window under "Debug" source.
            Trace.WriteLine(message);

            // Running test on the Build Agent, will display the output in the build output (console).
            Console.WriteLine(message);

            // Logging to xUnit test output.
            // Will display output associated with individual tests in xUnit. It will fail if it is called
            // when no test is active. For this reason we should set it in the test class constructor,
            // and set it to null in test class Dispose.
            // Test runners usually show the xUnit test output with the test result.
            // Running the command 'dotnet test --logger "console;verbosity=detailed"' will show the
            // test output in the console.
            if (TestOutputHelper != null)
            {
                TestOutputHelper.WriteLine(message);
            }
        }
    }
}
