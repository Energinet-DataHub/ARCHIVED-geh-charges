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
using Energinet.DataHub.Core.SchemaValidation;

namespace GreenEnergyHub.Charges.Infrastructure.CimDeserialization
{
    public static class XmlReaderExtension
    {
        public static bool Is(
            this SchemaValidatingReader reader,
            string localName,
            NodeType nodeType = NodeType.StartElement)
        {
            return reader.CurrentNodeName.Equals(localName) &&
                   reader.CurrentNodeType == nodeType;
        }

        public static bool IsElement(this SchemaValidatingReader reader)
        {
            return reader.CurrentNodeType == NodeType.StartElement;
        }

        public static async Task ReadUntilEoFOrNextElementNameAsync(
            this SchemaValidatingReader reader,
            string localName,
            NodeType xmlNodeType = NodeType.StartElement)
        {
            while (await reader.AdvanceAsync().ConfigureAwait(false))
            {
                if (reader.Is(
                        localName))
                {
                    break;
                }
            }
        }
    }
}
