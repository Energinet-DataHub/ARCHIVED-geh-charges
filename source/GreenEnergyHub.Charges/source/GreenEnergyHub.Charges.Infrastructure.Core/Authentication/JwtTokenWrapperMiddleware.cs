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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Energinet.DataHub.Core.App.FunctionApp.Middleware;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;

namespace GreenEnergyHub.Charges.Infrastructure.Core.Authentication
{
    /// <summary>
    /// Temporary middleware workaround to suppress authentication requirements on selected
    /// HTTP endpoints.
    ///
    /// The idea is to not register <see cref="JwtTokenMiddleware"/> as middleware but rather invoke
    /// it from this wrapping middleware if authentication is required. This is because the
    /// <see cref="JwtTokenMiddleware"/> currently doesn't support configuration of which endpoints
    /// to authenticate.
    /// </summary>
    public class JwtTokenWrapperMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly JwtTokenMiddleware _jwtTokenMiddleware;

        // Health check endpoints must allow anonymous access so we can use them with Azure monitoring:
        // https://docs.microsoft.com/en-us/azure/app-service/monitor-instances-health-check#authentication-and-security
        private readonly List<string> _functionNamesToExclude = new() { "HealthCheck", "HealthStatus", "SynchronizeFromMarketParticipantRegistry" };

        public JwtTokenWrapperMiddleware(JwtTokenMiddleware jwtTokenMiddleware)
        {
            _jwtTokenMiddleware = jwtTokenMiddleware;
        }

        public async Task Invoke(FunctionContext context, [NotNull] FunctionExecutionDelegate next)
        {
            var allowAnonymous = _functionNamesToExclude.Contains(context.FunctionDefinition.Name);
            if (allowAnonymous)
            {
                await next(context).ConfigureAwait(false);
                return;
            }

            await _jwtTokenMiddleware.Invoke(context, next).ConfigureAwait(false);
        }
    }
}
