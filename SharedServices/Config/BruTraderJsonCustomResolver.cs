using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BruSoftware.SharedServices.Config;

/// <summary>
/// Ref https://www.newtonsoft.com/json/help/html/ConditionalProperties.htm
/// </summary>
public class BruTraderJsonCustomResolver : JsonCustomResolver
{
    //public new static readonly CoreJsonContractResolver Instance = new CoreJsonContractResolver();

    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        var property = base.CreateProperty(member, memberSerialization);

        //if (property.DeclaringType == typeof(FilterId) && property.PropertyName == "Signals")
        //{
        //    property.ShouldSerialize = x => false;
        //}
        return property;
    }
}