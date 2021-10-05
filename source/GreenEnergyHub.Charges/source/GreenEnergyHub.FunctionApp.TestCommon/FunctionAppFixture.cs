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
using System.Collections.Generic;
using System.Threading.Tasks;
using GreenEnergyHub.FunctionApp.TestCommon.FunctionAppHost;
using GreenEnergyHub.TestCommon.Diagnostics;
using Microsoft.Extensions.Configuration;
using Xunit;
using Xunit.Abstractions;

namespace GreenEnergyHub.FunctionApp.TestCommon
{
    /// <summary>
    /// An xUnit fixture for supporting integration testing of an Azure Function App (container of functions).
    ///
    /// By inheriting from this and override it's hooks, it can be used to ensure:
    ///  * The Azure Functions host is started and the Azure functions are ready for serving requests.
    ///  * The host and integration tests uses the same instances of resources (e.g. a database instance).
    /// </summary>
    public abstract class FunctionAppFixture : IAsyncLifetime
    {
        protected FunctionAppFixture()
        {
            RandomSuffix = $"{DateTimeOffset.UtcNow:yyyy.MM.ddTHH.mm.ss}-{Guid.NewGuid()}";

            TestLogger = new TestDiagnosticsLogger();

            HostConfigurationBuilder = new FunctionAppHostConfigurationBuilder();
            HostStartupLog = new List<string>();

            var hostSettings = HostConfigurationBuilder.CreateFunctionAppHostSettings();
            OnConfigureHostSettings(hostSettings);
            HostManager = new FunctionAppHostManager(hostSettings, TestLogger);
        }

        public FunctionAppHostManager HostManager { get; }

        public ITestDiagnosticsLogger TestLogger { get; }

        /// <summary>
        /// Can be used as suffix when building names for eg. dynamically created queue/topics.
        /// </summary>
        public string RandomSuffix { get; }

        protected FunctionAppHostConfigurationBuilder HostConfigurationBuilder { get; }

        private IReadOnlyList<string> HostStartupLog { get; set; }

        public async Task InitializeAsync()
        {
            OnConfigureEnvironment();

            var localSettingsSnapshot = HostConfigurationBuilder.BuildLocalSettingsConfiguration();
            await OnInitializeFunctionAppDependenciesAsync(localSettingsSnapshot);

            try
            {
                HostManager.StartHost();
            }
            catch (Exception ex)
            {
                HostStartupLog = HostManager.GetHostLogSnapshot();
                await OnFunctionAppHostFailedAsync(HostStartupLog, ex);

                // Rethrow
                throw;
            }

            HostStartupLog = HostManager.GetHostLogSnapshot();
            await OnFunctionAppHostStartedAsync(HostStartupLog);
        }

        public async Task DisposeAsync()
        {
            HostManager.Dispose();

            await OnDisposeFunctionAppDependenciesAsync();
        }

        public string CreateUserRandomName(string commonNamePart)
        {
            return string.IsNullOrWhiteSpace(commonNamePart)
                ? throw new ArgumentException("Value cannot be null or whitespace.", nameof(commonNamePart))
                : $"{Environment.UserName}-{commonNamePart}-{RandomSuffix}";
        }

        /// <summary>
        /// Use this method to attach <paramref name="testOutputHelper"/> to the host logging pipeline.
        /// While attached, any entries written to host log pipeline will also be logged to xUnit test output.
        /// It is important that it is only attached while a test i active. Hence, it should be attached in
        /// the test class constructor; and detached in the test class Dispose method (using 'null').
        /// </summary>
        /// <param name="testOutputHelper">If a xUnit test is active, this should be the instance of xUnit's <see cref="ITestOutputHelper"/>; otherwise it should be 'null'.</param>
        public void SetTestOutputHelper(ITestOutputHelper testOutputHelper)
        {
            TestLogger.TestOutputHelper = testOutputHelper;
        }

        /// <summary>
        /// Configure host settings to match the name, framework and configuration (debug/release) of the
        /// function app under test.
        /// </summary>
        protected abstract void OnConfigureHostSettings(FunctionAppHostSettings hostSettings);

        /// <summary>
        /// Before starting the host or creating supporting manager/services, we set environment variables
        /// to e.g. ensure the host uses the same instances of resources (e.g. a database instance).
        /// </summary>
        protected virtual void OnConfigureEnvironment()
        {
        }

        /// <summary>
        /// Settings have been frozen, meaning loaded settings will not get updated
        /// if environment variables are changed from here on.
        /// </summary>
        /// <param name="localSettingsSnapshot">Loaded settings from "local.settings.json",
        /// which might have been overriden using environment variables.</param>
        protected virtual Task OnInitializeFunctionAppDependenciesAsync(IConfiguration localSettingsSnapshot)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Function App Host started.
        /// </summary>
        /// <param name="hostLogSnapshot">Contains snapshot of the output generated by the Function App Host during startup. There is not need to log this to output, as this happens already.</param>
        protected virtual Task OnFunctionAppHostStartedAsync(IReadOnlyList<string> hostLogSnapshot)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Function App Host failed during startup.
        /// </summary>
        /// <param name="hostLogSnapshot">Contains snapshot of the output generated by the Function App Host during startup. There is not need to log this to output, as this happens already.</param>
        /// <param name="exception">The exception thrown during startup.</param>
        protected virtual Task OnFunctionAppHostFailedAsync(IReadOnlyList<string> hostLogSnapshot, Exception exception)
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnDisposeFunctionAppDependenciesAsync()
        {
            return Task.CompletedTask;
        }
    }
}
