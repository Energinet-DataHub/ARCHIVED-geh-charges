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
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GreenEnergyHub.Charges.Core
{
    public class InterfaceConverter<T, TI1, TI2> : JsonConverter<T>
        where T : class
    {
        private readonly List<Type> _expectedTypes = new() { typeof(TI1), typeof(TI2) };

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var readerClone = reader;
            if (readerClone.TokenType != JsonTokenType.StartObject)
                ThrowJsonException("Problem in Start object!");

            readerClone.Read();
            if (readerClone.TokenType != JsonTokenType.PropertyName)
                ThrowJsonException("Token Type not equal to property name!");

            var propertyName = readerClone.GetString();
            if (string.IsNullOrWhiteSpace(propertyName) || propertyName != "$intent")
                ThrowJsonException("Unable to get $intent!");

            readerClone.Read();
            if (readerClone.TokenType != JsonTokenType.String)
                ThrowJsonException("Token Type is not JsonTokenString!");

            var intentValue = readerClone.GetString();
            if (string.IsNullOrWhiteSpace(intentValue))
                ThrowJsonException("IntentValue is null or empty string!");

            var intent = _expectedTypes.FirstOrDefault(x => x.Name == intentValue);
            if (intent == null)
                ThrowJsonException("IntentValue does not match any expected intent!");

            var deserialized = JsonSerializer.Deserialize(ref reader, intent!, options);
            if (deserialized == null)
                ThrowJsonException("De-Serialized object is null here!");

            return (T)deserialized;
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case null:
                    JsonSerializer.Serialize(writer, (T)null!, options);
                    break;
                default:
                    {
                        var type = value.GetType();
                        using var jsonDocument = JsonDocument.Parse(JsonSerializer.Serialize(value, type, options));
                        writer.WriteStartObject();
                        writer.WriteString("$intent", type.Name);

                        foreach (var element in jsonDocument.RootElement.EnumerateObject())
                        {
                            element.WriteTo(writer);
                        }

                        writer.WriteEndObject();
                        break;
                    }
            }
        }

        private void ThrowJsonException(string errorMessage)
        {
            throw new JsonException($"{errorMessage} method: {nameof(Read)} class :{nameof(InterfaceConverter<T, TI1, TI2>)}");
        }
    }
}
