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

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Energinet.Charges.Contracts;
using Energinet.DataHub.Core.Messaging.Protobuf;
using FluentAssertions;
using GreenEnergyHub.Charges.Infrastructure.Messaging;
using GreenEnergyHub.Charges.Infrastructure.Messaging.Registration;
using GreenEnergyHub.Charges.Infrastructure.Messaging.Serialization;
using GreenEnergyHub.TestHelpers;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Messaging.Registration
{
    [UnitTest]
    public class ServiceCollectionProtobufExtensionsTests
    {
        [Theory]
        [InlineAutoDomainData]
        public void ReceiveProtobufMessage_WhenCalled_NeededTypesCanBeResolved(
            [NotNull] Container sut)
        {
            // Act
            sut.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
            sut.ReceiveProtobufMessage<CreateDefaultChargeLinks>(
                configuration => configuration.WithParser(() => CreateDefaultChargeLinks.Parser));

            // Assert
            var actualRegistrations = sut.Collection.Container.GetCurrentRegistrations();
            actualRegistrations.Any(p =>
                p.ServiceType == typeof(MessageExtractor<CreateDefaultChargeLinks>)).Should().BeTrue();
            actualRegistrations.Any(p =>
                p.ServiceType == typeof(MessageDeserializer<CreateDefaultChargeLinks>)).Should().BeTrue();
            actualRegistrations.Any(p =>
                p.ServiceType == typeof(ProtobufInboundMapper<CreateDefaultChargeLinks>)).Should().BeTrue();
            actualRegistrations.Any(p =>
                p.ServiceType == typeof(ProtobufParser<CreateDefaultChargeLinks>)).Should().BeTrue();
        }
    }
}
