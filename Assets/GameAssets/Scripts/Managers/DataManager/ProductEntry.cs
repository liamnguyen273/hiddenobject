using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace com.tinycastle.StickerBooker
{
    public class ProductEntry
    {
        [JsonProperty("id")] public string Id;
        [JsonProperty("listingName")] public string ListingName;
        [JsonProperty("description")] public string Description;
        [JsonProperty("type")] public string Type;
        [JsonProperty("isConsumable")] public bool IsConsumable;

        [JsonProperty("isIap")] public bool IsIAP;  // If IAP, use service's catalog. Otherwise use the below fields
        [JsonProperty("price")] public int Price;
        [JsonProperty("currency")] public string Currency;
        
        [JsonProperty("buyResolution")] public string BuyResolution; // Also serves as type
        [JsonIgnore] private string[] ParsedBuyResolutions;
        [JsonProperty("resolutionData")] public string ResolutionData;
        [JsonIgnore] private string[] ParsedResolutionData;

        public string[] GetParsedBuyResolutions()
        {
            if (ParsedBuyResolutions == null)
            {
                ParsedBuyResolutions = BuyResolution?.Split(",", StringSplitOptions.RemoveEmptyEntries);
            }

            return ParsedBuyResolutions;
        }

        public string[] GetParsedResolutionData()
        {
            if (ParsedResolutionData == null)
            {
                ParsedResolutionData = ResolutionData?.Split(",", StringSplitOptions.RemoveEmptyEntries);
            }

            return ParsedResolutionData;
        }
        
        public static bool TryParseResource(string input, out (string resourceId, int count) item)
        {
            // Format: "id count
            var tokens = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            try
            {
                if (tokens.Length != 2)
                {
                    throw new Exception(
                        $"Parsing returns {tokens.Length} tokens (requires exactly 2), cannot parse further.");
                }

                var id = tokens[0];
                var count = int.Parse(tokens[1]);
                item = (id, count);
                return true;
            }
            catch (Exception e)
            {
                item = ("", -1);
                return false;
            }
        }

        public static int TryParseResources(string[] inputs, out (string resourceId, int count)[] items)
        {
            var total = 0;
            var list = new List<(string, int)>();
            foreach (var input in inputs)
            {
                var success = TryParseResource(input, out var item);

                if (success)
                {
                    list.Add(item);
                }

                total += success ? 1 : 0;
            }

            items = list.ToArray();
            return total;
        }
    }

    public class ProductEntryJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // Dd nothing
        }

        public override bool CanWrite => false;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null) return null;
            
            var obj = JObject.Load(reader);
            var val = new ProductEntry
            {
                Id = (string)obj["id"],
                ListingName = (string)obj["listingName"],
                Description = (string)obj["description"],
                Type = (string)obj["type"],
                IsConsumable = (bool)obj["isConsumable"],
                IsIAP = (bool)obj["isIap"],
                Price = (int)obj["price"],
                Currency = (string)obj["currency"],
                BuyResolution = (string)obj["buyResolution"],
                ResolutionData = (string)obj["resolutionData"]
            };

            return val;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ProductEntry);
        }
    }
}