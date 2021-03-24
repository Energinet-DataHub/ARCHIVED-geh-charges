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
using System.Globalization;
using AutoFixture.Idioms;

namespace GreenEnergyHub.Messaging.Protobuf.Tests.Tooling.AutoFixture.Behaviors
{
    /// <summary>
    /// Encapsulates expectations about the behavior of a method or constructor when it's invoked
    /// with a null argument, or an empty value for strings.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The ArgumentNullAndArgumentExceptionBehaviorExpectation class encapsulates the following expectation: when an
    /// action (such as a method call or constructor invocation) is performed with a
    /// <see langword="null"/> argument it should raise an <see cref="ArgumentNullException" />. If a <see cref="string" />
    /// argument is empty it should raise an <see cref="ArgumentException"/>> If either of these happens the expectation
    /// is verified. If no exception, or any other type of exception, is thrown the expectation isn't met.
    /// </para>
    /// </remarks>
    public class ArgumentNullAndArgumentExceptionBehaviorExpectation : IBehaviorExpectation
    {
        /// <summary>
        /// Verifies that the command behaves correct when invoked with a null argument.
        /// </summary>
        /// <param name="command">The command whose behavior must be examined.</param>
        /// <remarks>
        /// <para>
        /// The Verify method attempts to invoke the <paramref name="command" /> instance's
        /// <see cref="IGuardClauseCommand.Execute" /> with <see langword="null" />. The expected
        /// result is that this action throws an <see cref="ArgumentNullException" /> with proper parameter name,
        /// in which case the expected behavior is considered verified. If a <see cref="string" /> argument is provided and
        /// an <see cref="ArgumentException"/> is thrown the expected behavior is also considered verified. If any other
        /// exception is thrown, or if no exception is thrown at all, the verification fails and an exception is thrown.
        /// </para>
        /// <para>
        /// The behavior is only asserted if the command's
        /// <see cref="IGuardClauseCommand.RequestedType" /> is nullable. In case of value types,
        /// no action is performed.
        /// </para>
        /// </remarks>
        public void Verify(IGuardClauseCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            if (!command.RequestedType.IsClass
                && !command.RequestedType.IsInterface)
            {
                return;
            }

            try
            {
                command.Execute(null);
            }
            catch (Exception exception) when
                (exception is ArgumentNullException
                || (exception is ArgumentException && command.RequestedType == typeof(string)))
            {
                var castedException = exception is ArgumentNullException
                    ? exception as ArgumentNullException
                    : exception as ArgumentException;

                if (string.Equals(castedException!.ParamName, command.RequestedParameterName, StringComparison.Ordinal))
                {
                    return;
                }

                throw command.CreateException(
                    "<nullOrEmpty>",
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Guard Clause prevented it, however the thrown exception contains invalid parameter name. Ensure you pass correct parameter name to the ArgumentNullException constructor.{0}. Expected parameter name: {1}{0}Actual parameter name: {2}",
                        Environment.NewLine,
                        command.RequestedParameterName,
                        castedException.ParamName),
                    exception);
            }
            catch (Exception e)
            {
                throw command.CreateException("null", e);
            }

            throw command.CreateException("null");
        }
    }
}
