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

namespace GreenEnergyHub.Charges.FunctionHost.RequestResponseMiddleware.Models
{
    internal class LogTags
    {
        private Dictionary<string, string> _queryTags = new();
        private Dictionary<string, string> _tags = new();

        public void ParseAndAddQueryTagsCollection(string queryKeyValue)
        {
            try
            {
                var jsonQueryCollection = JsonSerializer.Deserialize<Dictionary<string, string>>(queryKeyValue);
                if (jsonQueryCollection != null)
                {
                    foreach (var item in jsonQueryCollection)
                    {
                        _queryTags.TryAdd(LogDataBuilder.MetaNameFormatter(item.Key), item.Value);
                        _tags.TryAdd(LogDataBuilder.MetaNameFormatter(item.Key), item.Value);
                    }
                }
            }
            catch (JsonException e)
            {
                _queryTags.TryAdd(LogDataBuilder.MetaNameFormatter("invalid query parsing"), e.Message);
                _tags.TryAdd(LogDataBuilder.MetaNameFormatter("invalid query parsing"), e.Message);
            }
        }

        public void AddContextTagsCollection(Dictionary<string, string> collection)
        {
            foreach (var item in collection)
            {
                _tags.TryAdd(LogDataBuilder.MetaNameFormatter(item.Key), item.Value);
            }
        }

        public void AddHeaderCollectionTags(Dictionary<string, string> collection)
        {
            foreach (var item in collection)
            {
                _tags.TryAdd(LogDataBuilder.MetaNameFormatter(item.Key), item.Value);
            }
        }

        public void AddToHeaderCollection(string key, string value)
        {
            _tags.TryAdd(LogDataBuilder.MetaNameFormatter(key), value);
        }

        public Dictionary<string, string> GetAllTags()
        {
            return _tags;
        }

        public Dictionary<string, string> GetQueryTags()
        {
            return _queryTags;
        }

        public Dictionary<string, string> BuildMetaDataForLog()
        {
            var jsonAll = JsonSerializer.Serialize(GetAllIndexTags());
            var jsonQuery = JsonSerializer.Serialize(GetQueryTags());

            var metaData = new Dictionary<string, string>(_tags);
            metaData.TryAdd(LogDataBuilder.MetaNameFormatter("indextags"), jsonAll);
            metaData.TryAdd(LogDataBuilder.MetaNameFormatter("querytags"), jsonQuery);
            return metaData;
        }

        public Dictionary<string, string> GetAllIndexTags()
        {
            var indexTagNames = IndexTagsKeys.GetKeys();
            var allTags = GetAllTags();

            var comparer = StringComparer.InvariantCultureIgnoreCase;
            var indexTags = allTags.Where(e => indexTagNames.Contains(e.Key, comparer));
            return indexTags.ToDictionary(e => e.Key, f => f.Value);
        }

        public Dictionary<string, string> GetIndexTagsWithMax10Items()
        {
            var indexTagNames = IndexTagsKeys.GetKeysForMax10Items();

            var allTags = GetAllTags();

            var comparer = StringComparer.InvariantCultureIgnoreCase;
            var indexTags = allTags
                .Where(e => indexTagNames.Contains(e.Key, comparer))
                .ToDictionary(e => e.Key, f => f.Value);

            var queryTagsToAdd = _queryTags.Take(10 - indexTags.Count);
            foreach (var (key, value) in queryTagsToAdd)
            {
                indexTags.TryAdd(key, value);
            }

            return indexTags;
        }
    }
}
