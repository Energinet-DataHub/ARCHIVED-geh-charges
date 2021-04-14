# Notice

Energinet and any contributors reserve all others rights, whether under their respective copyrights, patents, licenses, or trademarks, whether by implication, estoppel or otherwise.

It is our intention to acknowledge all third-party product licenses used in this document.
If you miss your a license please contact greenenergyhub@energinet.dk, and we will take action to change the file.

## Third-Party Licenses

The domain relies on open source libraries and tools.
We recommend you read their licenses, as their terms may differ from the terms described in our [LICENSE](LICENSE).
Microsoft NuGet packages have been omitted from this list.

### GitHub Actions

| Name | Version | Url | License |
| -- | -- | -- | -- |

<!---
  Table content created using this command:

  grep -rh " uses: " .github/**/*.y*ml | sed 's/[ -]*uses: //g' | grep -v "./.github/actions" | sort | sed 's/\(.*\)@\(.*\)/| `\1@\2` | <https:\/\/github.com\/\1> | MIT |/'
--->
| Action | Url | License |
| -- | -- | -- |
| `actions/checkout@master` | <https://github.com/actions/checkout> | MIT |
| `actions/checkout@v2` | <https://github.com/actions/checkout> | MIT |
| `actions/download-artifact@v2` | <https://github.com/actions/download-artifact> | MIT |
| `actions/setup-dotnet@v1` | <https://github.com/actions/setup-dotnet> | MIT |
| `actions/setup-python@v2` | <https://github.com/actions/setup-python> | MIT |
| `actions/upload-artifact@v2` | <https://github.com/actions/upload-artifact> | MIT |
| `avto-dev/markdown-lint@v1` | <https://github.com/avto-dev/markdown-lint> | MIT |
| `Azure/functions-action@v1` | <https://github.com/Azure/functions-action> | MIT |
| `fkirc/skip-duplicate-actions@v1.4.0` | <https://github.com/fkirc/skip-duplicate-actions> | MIT |
| `gaurav-nelson/github-action-markdown-link-check@v1` | <https://github.com/gaurav-nelson/github-action-markdown-link-check> | MIT |
| `hashicorp/setup-terraform@v1.2.1` | <https://github.com/hashicorp/setup-terraform> | MPL-2.0 |
| `kt3k/license_checker@v1.0.3` | <https://github.com/kt3k/license_checker> | MIT |
| `peter-evans/repository-dispatch@v1` | <https://github.com/peter-evans/repository-dispatch> | MIT |
| `rojopolis/spellcheck-github-actions@0.5.0` | <https://github.com/rojopolis/spellcheck-github-actions> | MIT |
| `xt0rted/markdownlint-problem-matcher@v1` | <https://github.com/xt0rted/markdownlint-problem-matcher> | MIT |

### NuGet Packages

| Name | Version | Url | License |
| -- | -- | -- | -- |
| `coverlet.collector` | 1.2.0 | <https://www.nuget.org/packages/coverlet.collector/1.2.0> | MIT |
| `Fluent Assertions` | 5.10.3 | <https://www.nuget.org/packages/FluentAssertions/5.10.3/> | Apache-2.0 |
| `NSubstitute` | 4.2.2 | <https://www.nuget.org/packages/NSubstitute/4.2.2> | BSD-3-Clause |
| `StyleCop.Analyzers` | 1.1.118 | <https://www.nuget.org/packages/StyleCop.Analyzers/1.1.118> | Apache-2.0 |
| `xunit` | 2.4.1 | <https://www.nuget.org/packages/xunit/2.4.1> | [`xunit` license](https://raw.githubusercontent.com/xunit/xunit/master/license.txt) |
| `xunit.runner.visualstudio` | 2.4.0 | <https://www.nuget.org/packages/xunit.runner.visualstudio/2.4.0> | [`xunit` license](https://raw.githubusercontent.com/xunit/xunit/master/license.txt) |