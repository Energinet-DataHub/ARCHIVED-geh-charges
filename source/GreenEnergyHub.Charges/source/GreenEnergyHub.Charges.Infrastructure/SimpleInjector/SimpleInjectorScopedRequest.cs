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
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace GreenEnergyHub.Charges.Infrastructure.SimpleInjector
{
    public sealed class SimpleInjectorScopedRequest : IFunctionsWorkerMiddleware
    {
        private readonly Container _container;

        public SimpleInjectorScopedRequest(Container container)
        {
            _container = container;
        }

        public async Task Invoke(FunctionContext context, [NotNull] FunctionExecutionDelegate next)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            await using var scope = AsyncScopedLifestyle.BeginScope(_container);
            if (scope.Container == null) throw new InvalidOperationException("Scope doesn't contain a container.");
            context.InstanceServices = new SimpleInjectorServiceProviderAdapter(scope.Container);
            await next(context).ConfigureAwait(false);
        }
    }
}
