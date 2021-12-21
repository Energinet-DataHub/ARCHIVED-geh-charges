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

using System.Threading.Tasks;
using System.Xml;

namespace GreenEnergyHub.Charges.Infrastructure.CimDeserialization
{
    public static class XmlReaderExtension
    {
        public static bool Is(
            this XmlReader reader,
            string localName,
            string ns,
            XmlNodeType xmlNodeType = XmlNodeType.Element)
        {
            return reader.LocalName.Equals(localName) && reader.NamespaceURI.Equals(ns) &&
                   reader.NodeType == xmlNodeType;
        }

        public static bool IsElement(this XmlReader reader)
        {
            return reader.NodeType == XmlNodeType.Element;
        }

        public static async Task ReadUntilEoFOrNextElementNameAsync(
            this XmlReader reader,
            string localName,
            string ns,
            XmlNodeType xmlNodeType = XmlNodeType.Element)
        {
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                if (reader.Is(
                        localName,
                        ns))
                {
                    break;
                }
            }
        }
    }
}
