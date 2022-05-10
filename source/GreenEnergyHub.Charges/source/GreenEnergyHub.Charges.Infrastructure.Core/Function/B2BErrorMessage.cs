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

using System.Text;
using System.Xml;

namespace GreenEnergyHub.Charges.Infrastructure.Core.Function
{
    public readonly struct B2BErrorMessage
    {
        public string Code { get; }

        public string Message { get; }

        public B2BErrorMessage(string code, string message)
        {
            Code = code;
            Message = message;
        }

        public string WriteAsXmlString()
        {
            var messageBody = new StringBuilder();
            var settings = new XmlWriterSettings() { OmitXmlDeclaration = true, Indent = true, };
            using var writer = XmlWriter.Create(messageBody, settings);
            writer.WriteStartElement("Error");
            writer.WriteElementString("Code", Code);
            writer.WriteElementString("Message", Message);
            writer.Close();
            return messageBody.ToString();
        }
    }
}
