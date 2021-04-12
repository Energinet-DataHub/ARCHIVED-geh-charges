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
    public static class AddBehaviorExtension
    {
        /// <summary>
        /// Add a mediator pipeline behavior
        /// </summary>
        /// <param name="services">Service container</param>
        /// <param name="pipeline">Pipeline configuration</param>
        /// <exception cref="ArgumentNullException"><paramref name="services"/> or <paramref name="pipeline"/> is null</exception>
        public static void AddPipelineBehavior(this IServiceCollection services, Action<IApplyPipelineBehavior> pipeline)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (pipeline == null) throw new ArgumentNullException(nameof(pipeline));

            pipeline.Invoke(new PipelineBehaviorConfiguration(services));
        }

        /// <summary>
        /// Add a scoped dependency
        /// </summary>
        /// <param name="configuration">Dependency configuration</param>
        /// <typeparam name="T">Type of dependency</typeparam>
        /// <exception cref="ArgumentNullException"><paramref name="configuration"/> is null</exception>
        public static DependsOnConfiguration On<T>(this DependsOnConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            return configuration.On(typeof(T));
        }
    }
}
