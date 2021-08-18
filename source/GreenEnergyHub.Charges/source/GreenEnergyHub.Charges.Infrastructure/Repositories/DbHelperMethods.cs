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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace GreenEnergyHub.Charges.Infrastructure.Repositories
{
    public static class DbHelperMethods
    {
        public static string ToSql<TEntity>(this IQueryable<TEntity> query)
            where TEntity : class
        {
            if (query != null)
            {
                using var enumerator = query.Provider.Execute<IEnumerable<TEntity>>(query.Expression).GetEnumerator();
                var relationalCommandCache = enumerator.Private("_relationalCommandCache");
                var selectExpression = relationalCommandCache.Private<SelectExpression>("_selectExpression");
                var factory = relationalCommandCache.Private<IQuerySqlGeneratorFactory>("_querySqlGeneratorFactory");

                var sqlGenerator = factory.Create();
                var command = sqlGenerator.GetCommand(selectExpression);

                string sql = command.CommandText;
                return sql;
            }

            throw new ArgumentException("crap");
        }

        private static object Private(this object obj, string privateField) => obj?.GetType().GetField(privateField, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(obj) !;

        private static T Private<T>(this object obj, string privateField) => ((T)obj?.GetType().GetField(privateField, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(obj) !) !;
    }
}
