using System;
using System.Reflection;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NLog;

namespace BruSoftware.SharedServices.Config;

/// <summary>
/// Thanks to http://stackoverflow.com/questions/20962316/ignoring-class-members-that-throw-exceptions-when-serializing-to-json
/// </summary>
public class JsonCustomResolver : DefaultContractResolver
{
    private static readonly Logger s_logger = LogManager.GetCurrentClassLogger();
    private static long s_counter;

    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        var property = base.CreateProperty(member, memberSerialization);
        var ignore = IgnoreProperty(member, property);
        if (!ignore)
        {
            property.ShouldSerialize = instance =>
            {
                try
                {
                    var prop = member as PropertyInfo;
                    if (prop == null)
                    {
                        return false;
                    }
                    if (prop.CanRead)
                    {
                        s_counter++;
                        if (s_counter > 50000)
                        {
                            // about to StackOverflow?
                        }
                        prop.GetValue(instance, null);
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    s_logger.Error(ex, "{Message}", ex.Message);
                }
                return false;
            };
        }

        return property;
    }

    /// <summary>
    /// Thanks to https://stackoverflow.com/questions/4686817/serialize-net-object-to-json-controlled-using-xml-attributes
    /// Ignore [XmlIgnore] in indicators etc. from NT8
    /// </summary>
    /// <param name="member"></param>
    /// <param name="property"></param>
    /// <returns></returns>
    private bool IgnoreProperty(MemberInfo member, JsonProperty property)
    {
        if (Attribute.IsDefined(member, typeof(XmlIgnoreAttribute), true))
        {
            property.Ignored = true;
            return true;
        }
        return false;
    }
}