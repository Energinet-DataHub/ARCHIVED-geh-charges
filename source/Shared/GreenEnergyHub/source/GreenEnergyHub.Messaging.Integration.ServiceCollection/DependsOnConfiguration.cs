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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GreenEnergyHub.Messaging
{
    public class DependsOnConfiguration
    {
        private readonly IServiceCollection _services;

        internal DependsOnConfiguration(IServiceCollection services)
        {
            _services = services;
        }

        /// <summary>
        /// Adds a scoped registration
        /// </summary>
        /// <param name="serviceType">Type of implementation</param>
        public DependsOnConfiguration On(Type serviceType)
        {
            _services.TryAddScoped(serviceType);
            return this;
        }
    }
}
