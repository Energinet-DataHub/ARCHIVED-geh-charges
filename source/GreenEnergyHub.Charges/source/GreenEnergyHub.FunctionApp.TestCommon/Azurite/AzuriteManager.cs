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
using System.Globalization;
using System.IO;
using System.Management;

namespace GreenEnergyHub.FunctionApp.TestCommon.Azurite
{
    /// <summary>
    /// Used to start Azurite, which is the storage emulator that replaced Azure Storage Emulator.
    /// Remember to dispose, otherwise the Azurite process wont be stopped.
    ///
    /// In most cases the AzuriteManager should be used in the FunctionAppFixture:
    /// - Create it in the constructor
    /// - Start it in OnInitializeFunctionAppDependenciesAsync()
    /// - Dispose it in OnDisposeFunctionAppDependenciesAsync()
    /// </summary>
    public class AzuriteManager : IDisposable
    {
        private Process? AzuriteProcess { get; set; }

        public void StartAzurite()
        {
            StopAzureStorageEmulator();
            StopHangingAzuriteProcess();
            StartAzuriteProcess();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            if (AzuriteProcess != null)
            {
                KillProcessAndChildrenRecursively(AzuriteProcess.Id);
                AzuriteProcess = null;
            }
        }

        private static void KillProcessAndChildrenRecursively(int processId)
        {
            var queryChildren = $"Select * From Win32_Process Where ParentProcessID = {processId}";
            using var childProcessManagementObjectSearcher = new ManagementObjectSearcher(queryChildren);
            using var childProcessManagementObjectCollection = childProcessManagementObjectSearcher.Get();

            foreach (var managementObject in childProcessManagementObjectCollection)
            {
                KillProcessAndChildrenRecursively(Convert.ToInt32(managementObject["ProcessID"], CultureInfo.InvariantCulture));
            }

            try
            {
                var process = Process.GetProcessById(processId);
                KillAndDisposeProcess(process);
            }
            catch (ArgumentException)
            {
                // Process already exited.
            }
        }

        /// <summary>
        /// Azure Storage Emulator is still started/used when starting the function app locally from some IDE's.
        /// </summary>
        private static void StopAzureStorageEmulator()
        {
            using var storageEmulatorProcess = new Process
            {
                StartInfo =
                {
                    FileName = @"C:\Program Files (x86)\Microsoft SDKs\Azure\Storage Emulator\AzureStorageEmulator.exe",
                    Arguments = "stop",
                },
            };

            var success = storageEmulatorProcess.Start();
            if (!success)
            {
                throw new InvalidOperationException("Error when stopping Azure Storage Emulator.");
            }

            // Azure Storage Emulator is stopped using a process that will exit right away.
            var timeout = TimeSpan.FromMinutes(2);
            var hasExited = storageEmulatorProcess.WaitForExit((int)timeout.TotalMilliseconds);
            if (!hasExited)
            {
                KillAndDisposeProcess(storageEmulatorProcess);
                throw new InvalidOperationException($"Azure Storage Emulator did not stop within: '{timeout}'");
            }
        }

        /// <summary>
        /// If test code has been used correctly there should be no hanging Azurite process.
        /// </summary>
        private static void StopHangingAzuriteProcess()
        {
            // The Azurite process is called "node.exe"
            var nodeProcesses = Process.GetProcessesByName("node");
            foreach (var nodeProcess in nodeProcesses)
            {
                var parentProcessId = GetParentProcessId(nodeProcess);
                try
                {
                    var parentProcess = Process.GetProcessById((int)parentProcessId);
                    if (!parentProcess.HasExited && parentProcess.ProcessName == "cmd")
                    {
                        // Warning: We assume that Azurite is the only Nodejs process started with cmd.exe
                        KillAndDisposeProcess(nodeProcess);
                    }
                }
                catch (ArgumentException)
                {
                    // Process.GetProcessById() throws ArgumentException when the process id has been killed
                    // We assume that Azurite is the only Nodejs process where the parent has been killed
                    KillAndDisposeProcess(nodeProcess);
                    break;
                }
            }
        }

        private static void KillAndDisposeProcess(Process process)
        {
            process.Kill();
            process.WaitForExit(milliseconds: 10000);
            process.Dispose();
        }

        private static uint GetParentProcessId(Process process)
        {
            var queryParentId = $"SELECT ParentProcessId FROM Win32_Process WHERE ProcessId = {process.Id}";
            using var parentIdMmanagementObjectSearcher = new ManagementObjectSearcher(queryParentId);
            using var searchResults = parentIdMmanagementObjectSearcher.Get().GetEnumerator();
            searchResults.MoveNext();
            using var queryObj = searchResults.Current;
            return (uint)queryObj["ParentProcessId"];
        }

        private void StartAzuriteProcess()
        {
            // When running locally a folder path is not needed because Azurite is installed globally (-g)
            var azuriteBlobFileName = "azurite-blob.cmd";
            var azuriteBlobFolderPath = Environment.GetEnvironmentVariable("AzuriteBlobFolderPath");
            var azuriteBlobFilePath = azuriteBlobFolderPath == null
                ? azuriteBlobFileName
                : Path.Combine(azuriteBlobFolderPath, azuriteBlobFileName);

            AzuriteProcess = new Process
            {
                StartInfo =
                {
                    FileName = azuriteBlobFilePath,
                    RedirectStandardError = true,
                },
            };
            try
            {
                var success = AzuriteProcess.Start();
                if (!success)
                {
                    throw new InvalidOperationException("Azurite failed to start");
                }
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Remember to install Azurite.\nAzurite failed to start: '{e.Message}'");
            }

            var hasExited = AzuriteProcess.WaitForExit(1000);
            if (hasExited)
            {
                var error = AzuriteProcess.StandardError.ReadToEnd();
                throw new InvalidOperationException($"Azurite failed to start: '{error}'.\nEnsure tests that are using Azurite are not running in parallel (use ICollectionFixture<TestFixture>).\nIf another process is using port 10000 then close that application.\nUse \"Get-Process -Id (Get-NetTCPConnection -LocalPort 10000).OwningProcess\" to find the other process.");
            }
        }
    }
}
