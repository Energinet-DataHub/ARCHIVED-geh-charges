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
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GreenEnergyHub.Messaging
{
    public sealed class PipelineBehaviorConfiguration : IContinuePipelineBehavior, IApplyPipelineBehavior
    {
        private static readonly Type _mediatorPipelineBehavior = typeof(IPipelineBehavior<,>);
        private readonly IServiceCollection _services;

        internal PipelineBehaviorConfiguration(IServiceCollection services)
        {
            _services = services;
        }

        /// <summary>
        /// Continue with a pipeline behavior
        /// </summary>
        /// <param name="pipelineBehavior">pipeline behavior to add</param>
        /// <param name="depends">Dependencies to register</param>
        /// <exception cref="ArgumentException"><paramref name="pipelineBehavior"/> does not inherit from <see cref="IPipelineBehavior{TRequest,TResponse}"/></exception>
        public IContinuePipelineBehavior Apply(Type pipelineBehavior, Action<DependsOnConfiguration>? depends = null)
        {
            if (pipelineBehavior == null) throw new ArgumentNullException(nameof(pipelineBehavior));
            if (!IsPipelineBehavior(pipelineBehavior))
            {
                throw new ArgumentException("This is not a pipeline behavior", nameof(pipelineBehavior));
            }

            AddBehavior(pipelineBehavior, depends);
            return this;
        }

        /// <summary>
        /// Apply a pipeline behavior
        /// </summary>
        /// <param name="pipelineBehavior">pipeline behavior to add</param>
        /// <param name="depends">Dependencies to register</param>
        /// <exception cref="ArgumentException"><paramref name="pipelineBehavior"/> does not inherit from <see cref="IPipelineBehavior{TRequest,TResponse}"/></exception>
        public IContinuePipelineBehavior ContinueWith(Type pipelineBehavior, Action<DependsOnConfiguration>? depends = null)
        {
            if (pipelineBehavior == null) throw new ArgumentNullException(nameof(pipelineBehavior));
            if (!IsPipelineBehavior(pipelineBehavior))
            {
                throw new ArgumentException("This is not a pipeline behavior", nameof(pipelineBehavior));
            }

            AddBehavior(pipelineBehavior, depends);
            return this;
        }

        private static bool IsPipelineBehavior(Type type)
        {
            return type.FindInterfaces(PipelineBehaviorFilter, _mediatorPipelineBehavior).Length == 1;
        }

        private static bool PipelineBehaviorFilter(Type type, object? objectCriteria)
        {
            if (objectCriteria == null) return false;
            if (type.IsGenericType == false) return false;

            return objectCriteria is Type objectType && objectType == type.GetGenericTypeDefinition();
        }

        private void AddBehavior(Type pipelineBehavior, Action<DependsOnConfiguration>? dependsConfig)
        {
            dependsConfig?.Invoke(new DependsOnConfiguration(_services));
            _services.AddScoped(typeof(IPipelineBehavior<,>), pipelineBehavior);
        }
    }
}
