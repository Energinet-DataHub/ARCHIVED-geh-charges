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
using System.Xml;

namespace GreenEnergyHub.Charges.MessageHub.Infrastructure
{
    public static class XmlReaderExtension
    {
        public static bool Is(
            [NotNull]this XmlReader reader,
            [NotNull]string localName,
            [NotNull]string ns,
            XmlNodeType xmlNodeType = XmlNodeType.Element)
        {
            return reader.LocalName.Equals(localName) && reader.NamespaceURI.Equals(ns) &&
                   reader.NodeType == xmlNodeType;
        }

        public static bool IsElement([NotNull] this XmlReader reader)
        {
            return reader.NodeType == XmlNodeType.Element;
        }
    }
}
