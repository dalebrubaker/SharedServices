using System;
using BruSoftware.SharedServices.Config;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BruSoftware.SharedServices;

/// <summary>
/// Thanks to https://stackoverflow.com/questions/31325866/newtonsoft-json-cannot-convert-model-with-typeconverter-attribute
/// </summary>
/// <typeparam name="T"></typeparam>
public class NoTypeJsonConverter<T> : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return typeof(T).IsAssignableFrom(objectType);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        return JsonSerializer.CreateDefault(ConfigJson.JsonSerializerSettings).Deserialize(reader, objectType);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        JsonSerializer.CreateDefault(ConfigJson.JsonSerializerSettings).Serialize(writer, value);
    }

    private class NoTypeConverterContractResolver : DefaultContractResolver
    {
        protected override JsonContract CreateContract(Type objectType)
        {
            if (typeof(T).IsAssignableFrom(objectType))
            {
                var contract = CreateObjectContract(objectType);
                contract.Converter = null; // Also null out the converter to prevent infinite recursion.
                return contract;
            }
            return base.CreateContract(objectType);
        }
    }
}