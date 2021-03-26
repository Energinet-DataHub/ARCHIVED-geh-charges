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
using System.Diagnostics;
using AutoFixture;
using AutoFixture.AutoMoq;

namespace GreenEnergyHub.Messaging.Protobuf.Tests.Tooling
{
    /// <summary>
    /// Base class for test classes, which provides a number of features:
    /// * Fixture, an instance of IFixture which provides access to AutoFixture configured with AutoMoq.
    /// * Use the <see cref="Sut"/> property to get a system-under-test instance.
    /// </summary>
    [DebuggerStepThrough]
    public abstract class TestBase<TSut>
        where TSut : class
    {
        protected TestBase()
            : this(new Fixture().Customize(new AutoMoqCustomization()))
        {
        }

        protected TestBase(IFixture fixture)
        {
            LazySut = new Lazy<TSut>(CreateSut);

            Fixture = fixture;
        }

        protected TSut Sut => LazySut.Value;

        protected IFixture Fixture { get; }

        private Lazy<TSut> LazySut { get; }

        protected virtual TSut CreateSut()
        {
            return Fixture.Create<TSut>();
        }
    }
}
