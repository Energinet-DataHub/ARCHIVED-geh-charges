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
using System.Linq.Expressions;
using System.Reflection;

namespace GreenEnergyHub.Charges.TestCore.Reflection
{
    public static class ReflectionExtensions
  {
    /// <summary>
    ///   Set property value. Example:
    ///   <code>
    ///     var myCustomerInstance = new Customer();
    ///     myCustomerInstance.SetPropertyValue(c => c.Title, "Mr");
    /// </code>
    ///   See https://stackoverflow.com/questions/9601707/how-to-set-property-value-using-expressions.
    /// </summary>
    public static void SetPrivateProperty<T, TValue>(this T target, Expression<Func<T, TValue>> memberLamda, TValue value)
    {
      var expression = memberLamda.Body;

      // Unbox if necessary
      if (memberLamda.Body is UnaryExpression unaryExpression && unaryExpression.NodeType == ExpressionType.Convert)
        expression = unaryExpression.Operand;

      if (expression is MemberExpression memberSelectorExpression)
      {
        var property = memberSelectorExpression.Member as PropertyInfo;
        if (property != null)
          property.SetValue(target, value, null);
      }
      else
      {
          throw new ArgumentException(
              $"Argument '{nameof(memberLamda)}' must have body of type '{nameof(MemberExpression)}'.");
      }
    }
  }
}
