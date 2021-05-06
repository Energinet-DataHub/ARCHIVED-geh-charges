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
using System.IO;
using System.Threading.Tasks;
using GreenEnergyHub.Json;
using Newtonsoft.Json;
using NodaTime;
using NodaTime.Serialization.JsonNet;

namespace GreenEnergyHub.Charges.Core.Json
{
    public class JsonSerializer : IJsonSerializer
    {
        private readonly Newtonsoft.Json.JsonSerializer _serializer;

        public JsonSerializer()
        {
            _serializer = new Newtonsoft.Json.JsonSerializer();
            _serializer.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
        }

        public ValueTask<object> DeserializeAsync(Stream utf8Json, Type returnType)
        {
            using var streamReader = new StreamReader(utf8Json);
            using var textReader = new JsonTextReader(streamReader);
            var result = _serializer.Deserialize(textReader, returnType);
            return new ValueTask<object>(result);
        }

        public TValue Deserialize<TValue>(string json)
        {
            using var textReader = new StringReader(json);
            using var jsonReader = new JsonTextReader(textReader);
            return _serializer.Deserialize<TValue>(jsonReader);
        }

        public string Serialize<TValue>(TValue value)
        {
            using var stringWriter = new StringWriter();
            using var textWriter = new JsonTextWriter(stringWriter);
            _serializer.Serialize(textWriter, value);
            textWriter.Flush();
            return stringWriter.ToString();
        }
    }
}
