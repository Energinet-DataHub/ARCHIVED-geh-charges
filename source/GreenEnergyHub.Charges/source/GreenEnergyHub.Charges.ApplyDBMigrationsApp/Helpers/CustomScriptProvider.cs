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
using DbUp.Engine;
using DbUp.Engine.Transactions;
using DbUp.ScriptProviders;

namespace GreenEnergyHub.Charges.ApplyDBMigrationsApp.Helpers
{
    public class CustomScriptProvider : EmbeddedScriptProvider, IScriptProvider
    {
        public CustomScriptProvider(Assembly assembly, Func<string, bool> filter)
            : base(assembly, filter)
        {
        }

        public new IEnumerable<SqlScript> GetScripts(IConnectionManager connectionManager)
        {
            var sqlScripts = base.GetScripts(connectionManager).ToList();

            var scriptNames = sqlScripts.Select(s => s.Name);
            foreach (var name in scriptNames)
            {
                if (!NamingConvention.Regex.IsMatch(name))
                {
                    throw new FormatException($"The script '{name}' doesn't adhere to the naming convention.");
                }
            }

            return sqlScripts;
        }
    }
}
