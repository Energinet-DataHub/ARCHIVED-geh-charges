// Decompiled with JetBrains decompiler
// Type: Energinet.DataHub.Core.Schemas.CimXml.CimXmlSchemaResolver
// Assembly: Energinet.DataHub.Core.Schemas, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FFFAB0C7-F79A-41C9-B970-400B0BDD7296
// Assembly location: C:\Users\HenrikSommer\.nuget\packages\energinet.datahub.core.schemas\1.0.9\lib\net5.0\Energinet.DataHub.Core.Schemas.dll

using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Schema;

#nullable enable
namespace GreenEnergyHub.Charges.Infrastructure.CimDeserialization.ChargeBundle
{
    internal sealed class TemporaryCimXmlSchemaResolver
    {
        private static readonly Assembly _currentAssembly = Assembly.GetExecutingAssembly();

        public Task<Stream> ResolveAsync(string? resourceName) => Task.FromResult<Stream>(TemporaryCimXmlSchemaResolver._currentAssembly.GetManifestResourceStream("GreenEnergyHub.Charges.Infrastructure.CimDeserialization.Schemas." + resourceName) ?? throw new XmlSchemaException("Could not resolve XML Schema named " + resourceName + "."));
    }
}
