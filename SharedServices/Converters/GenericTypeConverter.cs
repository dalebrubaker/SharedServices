using System;
using System.ComponentModel;
using NLog;

namespace BruSoftware.SharedServices.Converters;

/// <summary>
/// Support sorted properties
/// </summary>
/// <typeparam name="T"></typeparam>
public class GenericConverter<T> : PropertySorter where T : IProperties
{
    private static readonly Logger s_logger = LogManager.GetCurrentClassLogger();

    public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
    {
        try
        {
            if (!(value is T propertiesClass))
            {
                return new PropertyDescriptorCollection(Array.Empty<PropertyDescriptor>());
            }
            return SortProperties(propertiesClass.Properties);
        }
        catch (Exception ex)
        {
            s_logger.Error(ex, "{Message}", ex.Message);
            throw;
        }
    }

    public override bool GetPropertiesSupported(ITypeDescriptorContext context)
    {
        return true;
    }
}