﻿// Copyright 2020 Energinet DataHub A/S
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
using System.Linq;
using System.Reflection;
using Microsoft.Azure.Functions.Worker;

namespace GreenEnergyHub.Charges.Tests.TestHelpers
{
    public static class DependencyHelper
    {
        public static IEnumerable<Type> GetConstructorParametersForDependency(Type dependency)
        {
            return GetParameterTypesForConstructor(dependency.GetConstructors().Single());
        }

        public static IEnumerable<Type> FindDependenciesForType(Type type)
        {
            return type.Assembly.GetTypes().Where(MethodsAreAnnotatedWithFunctionAttribute);
        }

        private static IEnumerable<Type> GetParameterTypesForConstructor(ConstructorInfo constructorInfo)
        {
            return constructorInfo.GetParameters().Select(pi => pi.ParameterType);
        }

        private static bool MethodsAreAnnotatedWithFunctionAttribute(Type type)
        {
            return type.GetMethods().Any(MethodIsAnnotatedWithFunctionAttribute);
        }

        private static bool MethodIsAnnotatedWithFunctionAttribute(MethodInfo methodInfo)
        {
            return methodInfo.IsDefined(typeof(FunctionAttribute));
        }
    }
}
