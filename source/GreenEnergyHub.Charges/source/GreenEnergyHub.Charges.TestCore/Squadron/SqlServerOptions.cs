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
using Squadron;

namespace GreenEnergyHub.Charges.TestCore.Squadron
{
    /// <summary>
    /// From Squadron code base
    /// </summary>
    public class SqlServerOptions : ContainerResourceOptions
    {
        public override void Configure(ContainerResourceBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            // Add "_" and upper case to and a random string to pass SQL Server password
            // policy requirements "N" removes '-' as they are not valid
            // See requirements at https://hub.docker.com/_/microsoft-mssql-server
            var password = "PQaz27_!" + Guid.NewGuid().ToString("N");
            builder
                .Name("mssql")
                .Image("mcr.microsoft.com/mssql/server:2019-latest")
                .InternalPort(1433)
                .Username("sa")
                .Password(password)
                .AddEnvironmentVariable("ACCEPT_EULA=Y")
                .AddEnvironmentVariable($"SA_PASSWORD={password}");
        }
    }
}
