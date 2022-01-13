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
using System.Linq;
using System.Reflection;
using GreenEnergyHub.Charges.FunctionHost;
using Microsoft.Azure.Functions.Worker;

namespace GreenEnergyHub.Charges.Tests.FunctionHost
{
    public class FunctionsValidator
    {
        private readonly FunctionsApp _functionsAppType;

        public FunctionsValidator(FunctionsApp functionsAppType)
        {
            _functionsAppType = functionsAppType;
        }

        public void Verify()
        {
            /*
             var host = _functionsAppType.Build();
             bool CanResolveFromServiceProvider(Type t) => host.Services.GetService(t) != null;
             */

            bool CanResolve(Type t) => true;
            var constructorValidator = new ConstructorValidator(CanResolve);
            var allFunctions = GetAllTypesWithMethodsAnnotatedWithAttribute<FunctionAttribute>(_functionsAppType.GetType().Assembly);

            foreach (var type in allFunctions)
            {
                if (!constructorValidator.Verify(type))
                    throw new InvalidOperationException($"Missing service registration for function '{type.FullName}'");
            }
        }

        internal static Type[] GetAllTypesWithMethodsAnnotatedWithAttribute<TAttribute>(Assembly assembly)
            where TAttribute : Attribute
        {
            var types = assembly.GetTypes()
                .Where(ContainsMethodsAnnotatedWith<TAttribute>)
                .ToArray();

            return types;
        }

        private static bool ContainsMethodsAnnotatedWith<TAttribute>(Type type)
            where TAttribute : Attribute
        {
            var containsFunctionName = type.GetMethods()
                .Any(m => m.GetCustomAttributes(typeof(TAttribute), true).Length > 0);

            return containsFunctionName;
        }
    }
}
