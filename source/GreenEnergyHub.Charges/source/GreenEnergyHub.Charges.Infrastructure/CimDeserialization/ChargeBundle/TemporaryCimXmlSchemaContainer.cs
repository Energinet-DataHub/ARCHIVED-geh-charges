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
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Xml.Schema;
using Energinet.DataHub.Core.SchemaValidation;

namespace GreenEnergyHub.Charges.Infrastructure.CimDeserialization.ChargeBundle
{
    public class TemporaryCimXmlSchemaContainer : IXmlSchema
    {
        public async Task<XmlSchemaSet> GetXmlSchemaSetAsync()
        {
            XmlSchemaSet xmlSchemaSet1 = new XmlSchemaSet();
            XmlSchemaSet xmlSchemaSet2 = xmlSchemaSet1;
            xmlSchemaSet2.Add(await TemporaryCimXmlSchemaContainer.LoadSchemaRecusivelyAsync(_resourceName).ConfigureAwait(false));
            xmlSchemaSet2 = (XmlSchemaSet)null!;
            XmlSchemaSet xmlSchemaSetAsync = xmlSchemaSet1;
            xmlSchemaSet1 = (XmlSchemaSet)null!;
            return xmlSchemaSetAsync;

            /*var filePath = "CimDeserialization/Schemas/urn-ediel-org-structure-requestchangeofpricelist-0-1.xsd";
            // using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            var stream = GetEmbeddedResourceAsStream(filePath);

            var xmlSchema = XmlSchema.Read(stream, null);
            var xmlSchemaSet = new XmlSchemaSet();
            await Task.Run(() => xmlSchemaSet.Add(xmlSchema!)).ConfigureAwait(false);
            return xmlSchemaSet;*/
        }

        private static readonly TemporaryCimXmlSchemaResolver _xmlResolver = new TemporaryCimXmlSchemaResolver();

        private static readonly ConcurrentDictionary<string, Task<XmlSchema>> _schemaCache = new ConcurrentDictionary<string, Task<XmlSchema>>();

        private readonly string _resourceName;

        public TemporaryCimXmlSchemaContainer(string resourceName) => _resourceName = resourceName;

        /*private static Stream GetEmbeddedResourceAsStream(string filePath)
        {
            var basePath = Assembly.GetExecutingAssembly().Location;
            var path = Path.Combine(Directory.GetParent(basePath)!.FullName, filePath);
            var stream = File.OpenRead(path);

            return stream;
        }*/

        private static Task<XmlSchema> GetXmlSchemaAsync(string location) => TemporaryCimXmlSchemaContainer._schemaCache.GetOrAdd(location, new Func<string, Task<XmlSchema>>(TemporaryCimXmlSchemaContainer.LoadSchemaRecusivelyAsync));

        private static async Task<XmlSchema> LoadSchemaRecusivelyAsync(string location)
        {
            XmlSchema xmlSchema = XmlSchema.Read(await TemporaryCimXmlSchemaContainer._xmlResolver.ResolveAsync(location).ConfigureAwait(false), (ValidationEventHandler)null!)!;
            foreach (XmlSchemaExternal include in xmlSchema.Includes)
            {
                if (include.SchemaLocation != null)
                {
                    XmlSchemaExternal xmlSchemaExternal = include;
                    xmlSchemaExternal.Schema = await TemporaryCimXmlSchemaContainer.GetXmlSchemaAsync(include.SchemaLocation).ConfigureAwait(false);
                    xmlSchemaExternal = (XmlSchemaExternal)null!;
                }
            }

            XmlSchema xmlSchema1 = xmlSchema;
            xmlSchema = (XmlSchema)null!;
            return xmlSchema1;
        }
    }
}
