# Notice

Energinet and any contributors reserve all others rights, whether under their respective copyrights, patents, licenses, or trademarks, whether by implication, estoppel or otherwise.

It is our intention to acknowledge all third-party product licenses used in this document.
If you miss your a license please contact greenenergyhub@energinet.dk, and we will take action to change the file.

## Third-Party Licenses

The domain relies on open source libraries and tools.
We recommend you read their licenses, as their terms may differ from the terms described in our [LICENSE](LICENSE).
Microsoft NuGet packages have been omitted from this list.

### GitHub Actions

<!---
  Table content created using this command:

      grep -rh " uses: " .github/**/*.y*ml | sed 's/[ -]*uses: //g' | grep -v "./.github/actions" | sort -u | sed 's/\(.*\)@v\?\(.*\)/| `\1` | \2 | <https:\/\/github.com\/\1> | MIT |/'

  Please fix LICENSE and add probably also (re)add Azure CLI in case you update the whole table.

    | `Azure CLI` | | <https://aka.ms/InstallAzureCLIDeb> | MIT |
--->
| Name | Version | Url | License |
| -- | -- | -- | -- |
| `actions/checkout` | master | <https://github.com/actions/checkout> | MIT |
| `actions/checkout` | 2 | <https://github.com/actions/checkout> | MIT |
| `actions/download-artifact` | 2 | <https://github.com/actions/download-artifact> | MIT |
| `actions/setup-dotnet` | 1 | <https://github.com/actions/setup-dotnet> | MIT |
| `actions/setup-python` | 2 | <https://github.com/actions/setup-python> | MIT |
| `actions/upload-artifact` | 2 | <https://github.com/actions/upload-artifact> | MIT |
| `avto-dev/markdown-lint` | 1 | <https://github.com/avto-dev/markdown-lint> | MIT |
| `Azure/functions-action` | 1 | <https://github.com/Azure/functions-action> | MIT |
| `Azure CLI` | | <https://aka.ms/InstallAzureCLIDeb> | MIT |
| `fkirc/skip-duplicate-actions` | 1.4.0 | <https://github.com/fkirc/skip-duplicate-actions> | MIT |
| `gaurav-nelson/github-action-markdown-link-check` | 1 | <https://github.com/gaurav-nelson/github-action-markdown-link-check> | MIT |
| `hashicorp/setup-terraform` | 1.2.1 | <https://github.com/hashicorp/setup-terraform> | MPL-2.0 |
| `kt3k/license_checker` | 1.0.3 | <https://github.com/kt3k/license_checker> | MIT |
| `peter-evans/repository-dispatch` | 1 | <https://github.com/peter-evans/repository-dispatch> | MIT |
| `rojopolis/spellcheck-github-actions` | 0.5.0 | <https://github.com/rojopolis/spellcheck-github-actions> | MIT |
| `xt0rted/markdownlint-problem-matcher` | 1 | <https://github.com/xt0rted/markdownlint-problem-matcher> | MIT |

### NuGet Packages

Below gives an overview of the NuGet packages used across the solutions in the repository.

#### Charges Solution (GreenEnergyHub.Charges)

| Name | Version | Url | License |
| -- | -- | -- | -- |
| `AutoFixture` | 4.16.0 | <https://www.nuget.org/packages/AutoFixture/4.16.0/> | MIT |
| `AutoFixture.AutoMoq` | 4.16.0 | <https://www.nuget.org/packages/AutoFixture.AutoMoq/4.16.0/> | MIT |
| `AutoFixture.Xunit2` | 4.16.0 | <https://www.nuget.org/packages/AutoFixture.Xunit2/4.16.0/> | MIT |
| `Confluent.Kafka` | 1.5.3 | <https://www.nuget.org/packages/Confluent.Kafka/1.5.3/> | Apache-2.0 |
| `coverlet.collector` | 3.0.3 | <https://www.nuget.org/packages/coverlet.collector/3.0.3/> | MIT |
| `dbup-sqlserver` | 4.5.0 | <https://www.nuget.org/packages/dbup-sqlserver/4.5.0/> | MIT |
| `FluentAssertions` | 5.10.3 | <https://www.nuget.org/packages/FluentAssertions/5.10.3/> | Apache-2.0 |
| `FluentValidation` | 9.5.4 | <https://www.nuget.org/packages/FluentValidation/9.5.4/> | Apache-2.0 |
| `MediatR` | 9.0.0 | <https://www.nuget.org/packages/MediatR/9.0.0/> | Apache-2.0 |
| `MediatR.Extensions.Microsoft.DependencyInjection` | 9.0.0 | <https://www.nuget.org/packages/MediatR.Extensions.Microsoft.DependencyInjection/9.0.0/> | Apache-2.0 |
| `Moq` | 4.16.1 | <https://www.nuget.org/packages/Moq/4.16.1/> | BSD-3-Clause |
| `NodaTime` | 3.0.5 | <https://www.nuget.org/packages/NodaTime/3.0.5/> | Apache-2.0 |
| `NodaTime.Serialization.SystemTextJson` | 1.0.0 | <https://www.nuget.org/packages/NodaTime.Serialization.SystemTextJson/1.0.0/> | Apache-2.0 |
| `NSubstitute` | 4.2.2 | <https://www.nuget.org/packages/NSubstitute/4.2.2> | BSD-3-Clause |
| `StyleCop.Analyzers` | 1.1.118 | <https://www.nuget.org/packages/StyleCop.Analyzers/1.1.118> | Apache-2.0 |
| `xunit` | 2.4.1 | <https://www.nuget.org/packages/xunit/2.4.1> | [`xunit` license](https://raw.githubusercontent.com/xunit/xunit/master/license.txt) |
| `xunit.runner.visualstudio` | 2.4.3 | <https://www.nuget.org/packages/xunit.runner.visualstudio/2.4.3> | [`xunit` license](https://raw.githubusercontent.com/xunit/xunit/master/license.txt) |

#### Messaging solution (GreenEnergyHub.Messaging)

This solution has been temporarily copied to the Charges repository. We expect this to be available as NuGet packages later.

| Name | Version | Url | License |
| -- | -- | -- | -- |
| `AutoFixture` | 4.15.0 | <https://www.nuget.org/packages/AutoFixture/4.15.0/> | MIT |
| `AutoFixture.AutoMoq` | 4.15.0 | <https://www.nuget.org/packages/AutoFixture.AutoMoq/4.15.0/> | MIT |
| `AutoFixture.Idioms` | 4.15.0 | <https://www.nuget.org/packages/AutoFixture.Idioms/4.15.0/> | MIT |
| `AutoFixture.Xunit2` | 4.15.0 | <https://www.nuget.org/packages/AutoFixture.Xunit2/4.15.0/> | MIT |
| `coverlet.collector` | 3.0.3 | <https://www.nuget.org/packages/coverlet.collector/3.0.3/> | MIT |
| `coverlet.msbuild` | 3.0.3 | <https://www.nuget.org/packages/coverlet.msbuild/3.0.3/> | MIT |
| `FluentAssertions` | 5.10.3 | <https://www.nuget.org/packages/FluentAssertions/5.10.3/> | Apache-2.0 |
| `FluentValidation` | 9.5.4 | <https://www.nuget.org/packages/FluentValidation/9.5.4/> | Apache-2.0 |
| `Google.Protobuf` | 3.15.1 | <https://www.nuget.org/packages/Google.Protobuf/3.15.1/> | [`protobuf` license](https://github.com/protocolbuffers/protobuf/blob/master/LICENSE) |
| `Grpc.Tools` | 2.35.0 | <https://www.nuget.org/packages/Grpc.Tools/2.35.0/> | Apache-2.0 |
| `MediatR` | 9.0.0 | <https://www.nuget.org/packages/MediatR/9.0.0/> | Apache-2.0 |
| `MediatR.Extensions.Microsoft.DependencyInjection` | 9.0.0 | <https://www.nuget.org/packages/MediatR.Extensions.Microsoft.DependencyInjection/9.0.0/> | Apache-2.0 |
| `Moq` | 4.16.0 | <https://www.nuget.org/packages/Moq/4.16.0/> | BSD-3-Clause |
| `Moq.AutoMock` | 2.3.0 | <https://www.nuget.org/packages/Moq.AutoMock/2.3.0/> | MIT |
| `NodaTime` | 3.0.5 | <https://www.nuget.org/packages/NodaTime/3.0.5/> | Apache-2.0 |
| `NodaTime.Serialization.Protobuf` | 2.0.0 | https://www.nuget.org/packages/NodaTime.Serialization.Protobuf/2.0.0/ | Apache-2.0 |
| `StyleCop.Analyzers` | 1.1.118 | <https://www.nuget.org/packages/StyleCop.Analyzers/1.1.118> | Apache-2.0 |
| `xunit` | 2.4.1 | <https://www.nuget.org/packages/xunit/2.4.1> | [`xunit` license](https://raw.githubusercontent.com/xunit/xunit/master/license.txt) |
| `xunit.runner.visualstudio` | 2.4.3 | <https://www.nuget.org/packages/xunit.runner.visualstudio/2.4.3> | [`xunit` license](https://raw.githubusercontent.com/xunit/xunit/master/license.txt) |

#### Queues solution (GreenEnergyHub.Queues)

This solution has been temporarily copied to the Charges repository. We expect this to be available as NuGet packages later.

| Name | Version | Url | License |
| -- | -- | -- | -- |
| `Confluent.Kafka` | 1.5.3 | <https://www.nuget.org/packages/Confluent.Kafka/1.5.3/> | Apache-2.0 |
| `coverlet.collector` | 1.2.0 | <https://www.nuget.org/packages/coverlet.collector/1.2.0/> | MIT |
| `coverlet.msbuild` | 3.0.0 | <https://www.nuget.org/packages/coverlet.msbuild/3.0.0/> | MIT |
| `FluentValidation` | 9.5.4 | <https://www.nuget.org/packages/FluentValidation/9.5.4/> | Apache-2.0 |
| `MediatR` | 9.0.0 | <https://www.nuget.org/packages/MediatR/9.0.0/> | Apache-2.0 |
| `Moq` | 4.16.0 | <https://www.nuget.org/packages/Moq/4.16.0/> | BSD-3-Clause |
| `Moq.AutoMock` | 2.3.0 | <https://www.nuget.org/packages/Moq.AutoMock/2.3.0/> | MIT |
| `NodaTime` | 3.0.5 | <https://www.nuget.org/packages/NodaTime/3.0.5/> | Apache-2.0 |
| `NodaTime.Serialization.SystemTextJson` | 1.0.0 | <https://www.nuget.org/packages/NodaTime.Serialization.SystemTextJson/1.0.0/> | Apache-2.0 |
| `StyleCop.Analyzers` | 1.1.118 | <https://www.nuget.org/packages/StyleCop.Analyzers/1.1.118> | Apache-2.0 |
| `xunit` | 2.4.0 | <https://www.nuget.org/packages/xunit/2.4.0> | [`xunit` license](https://raw.githubusercontent.com/xunit/xunit/master/license.txt) |
| `xunit.runner.visualstudio` | 2.4.0 | <https://www.nuget.org/packages/xunit.runner.visualstudio/2.4.0> | [`xunit` license](https://raw.githubusercontent.com/xunit/xunit/master/license.txt) |

#### Shared solution (GreenEnergyHub)

This solution has been temporarily copied to the Charges repository. We expect this to be available as NuGet packages later.

| Name | Version | Url | License |
| -- | -- | -- | -- |
| `` | -- | <> | -- |
| `AutoFixture` | 4.16.0 | <https://www.nuget.org/packages/AutoFixture/4.16.0/> | MIT |
| `AutoFixture.AutoMoq` | 4.16.0 | <https://www.nuget.org/packages/AutoFixture.AutoMoq/4.16.0/> | MIT |
| `AutoFixture.Xunit2` | 4.16.0 | <https://www.nuget.org/packages/AutoFixture.Xunit2/4.16.0/> | MIT |
| `FluentAssertions` | 5.10.3 | <https://www.nuget.org/packages/FluentAssertions/5.10.3/> | Apache-2.0 |
| `Moq` | 4.16.1 | <https://www.nuget.org/packages/Moq/4.16.1/> | BSD-3-Clause |
| `NodaTime.Serialization.SystemTextJson` | 1.0.0 | <https://www.nuget.org/packages/NodaTime.Serialization.SystemTextJson/1.0.0/> | Apache-2.0 |
| `StyleCop.Analyzers` | 1.1.118 | <https://www.nuget.org/packages/StyleCop.Analyzers/1.1.118> | Apache-2.0 |
| `xunit` | 2.4.1 | <https://www.nuget.org/packages/xunit/2.4.1> | [`xunit` license](https://raw.githubusercontent.com/xunit/xunit/master/license.txt) |
| `xunit.runner.visualstudio` | 2.4.3 | <https://www.nuget.org/packages/xunit.runner.visualstudio/2.4.3> | [`xunit` license](https://raw.githubusercontent.com/xunit/xunit/master/license.txt) |
