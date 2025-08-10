// unset
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BruSoftware.SharedServices.Config;

/// <summary>
/// Thanks to https://stackoverflow.com/questions/31325866/newtonsoft-json-cannot-convert-model-with-typeconverter-attribute
/// </summary>
public class NoTypeConverterJsonConverter<T> : JsonConverter
{
    private static readonly IContractResolver resolver = new NoTypeConverterContractResolver();

    public override bool CanConvert(Type objectType)
    {
        return typeof(T).IsAssignableFrom(objectType);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        return JsonSerializer.CreateDefault(new JsonSerializerSettings { ContractResolver = resolver }).Deserialize(reader, objectType);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        JsonSerializer.CreateDefault(new JsonSerializerSettings { ContractResolver = resolver }).Serialize(writer, value);
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