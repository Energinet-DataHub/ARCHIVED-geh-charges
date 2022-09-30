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

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace GreenEnergyHub.Charges.IntegrationTest.Core.TestHelpers
{
    public class CIMXmlReader
    {
        public static IList<string> GetActivityRecords(string xml)
        {
            var document = XDocument.Parse(RemoveByteOrderMark(xml));
            var ns = document.Root!.GetNamespaceOfPrefix("cim")!;
            var marketActivityRecords = document.Descendants(ns + "MktActivityRecord");

            return marketActivityRecords
                .Select(mar => mar.Element(ns + "originalTransactionIDReference_MktActivityRecord.mRID"))
                .Select(elm => elm!.Value.ToString())
                .ToList();
        }

        private static string RemoveByteOrderMark(string xml)
        {
            var byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
            if (xml.StartsWith(byteOrderMarkUtf8))
            {
                xml = xml.Remove(0, byteOrderMarkUtf8.Length);
            }

            return xml;
        }
    }
}
