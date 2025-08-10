using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using BruSoftware.SharedServices.Attributes;
using BruSoftware.SharedServices.Converters;
using NLog;

// ReSharper disable once CheckNamespace
namespace BruSoftware.SharedServices;

public static class TypeExtensions
{
    private static readonly Logger s_logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Thanks to Marc Gravell http://stackoverflow.com/questions/2051834/exclude-property-from-gettype-getproperties
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static PropertyInfo[] GetFilteredProperties(this Type type)
    {
        var properties = type.GetProperties();
        var result = properties.Where(pi => !Attribute.IsDefined(pi, typeof(SkipPropertyAttribute))).ToArray();
        return result;
    }

    public static List<PropertyDescriptorExtended> GetBrowsablePropertyDescriptors(this Type type)
    {
        var properties = TypeDescriptor.GetProperties(type);
        var result = new List<PropertyDescriptorExtended>(properties.Count);
        foreach (PropertyDescriptor property in properties)
        {
            var attr = property.Attributes.OfType<BrowsableAttribute>().FirstOrDefault();
            if (attr != null && !attr.Browsable)
            {
                continue;
            }
            result.Add(new PropertyDescriptorExtended(property));
        }
        return result;
    }

    public static List<PropertyDescriptorExtended> GetAllPropertyDescriptors(this Type type)
    {
        var properties = TypeDescriptor.GetProperties(type);
        var result = new List<PropertyDescriptorExtended>(properties.Count);
        foreach (PropertyDescriptor property in properties)
        {
            result.Add(new PropertyDescriptorExtended(property));
        }
        return result;
    }

    public static List<PropertyDescriptor> GetPropertyDescriptorsWithAttribute(this Type type, Attribute attribute)
    {
        var attributeName = attribute.GetType().Name;
        var properties = TypeDescriptor.GetProperties(type);
        var propertyDescriptors = new List<PropertyDescriptor>(properties.Count);
        for (var i = 0; i < properties.Count; i++)
        {
            var property = properties[i];
            foreach (Attribute propertyAttribute in property.Attributes)
            {
                if (propertyAttribute.GetType().Name == attributeName)
                {
                    propertyDescriptors.Add(property);
                    break;
                }
            }
        }
        return propertyDescriptors;
    }

    /// <summary>
    /// Convert the list of CategoryOrderAttributes on a class to a dictionary of CategorySortedAttribute by category name
    /// </summary>
    /// <param name="type">the class</param>
    /// <returns></returns>
    public static Dictionary<string, CategorySortedAttribute> GetCategorySortedAttributesDictionary(this Type type)
    {
        var classAttributes = type.GetCustomAttributes(true);
        var categoryByOrderSortedDictionary = new SortedDictionary<int, string>();
        foreach (Attribute classAttribute in classAttributes)
        {
            if (classAttribute is CategoryOrderAttribute categoryOrderAttribute)
            {
                // Allow overwrite by duplicates having the same order
                categoryByOrderSortedDictionary[categoryOrderAttribute.Order] = categoryOrderAttribute.Category;
            }
        }
        var result = new Dictionary<string, CategorySortedAttribute>();
        var keys = categoryByOrderSortedDictionary.Keys.ToList();
        for (var i = 0; i < categoryByOrderSortedDictionary.Count; i++)
        {
            var order = keys[i];
            var categoryName = categoryByOrderSortedDictionary[order];
            if (result.ContainsKey(categoryName))
            {
                // Use the earlier order defined by the indicator rather than our default
                continue;
            }
            var categorySorted = new CategorySortedAttribute(categoryName, i, keys.Count);
            result[categoryName] = categorySorted; // override any existing key with the new one
        }
        return result;
    }

    public static PropertyDescriptorExtended ConvertDisplayAttributeForWinForms(this PropertyDescriptor propertyDescriptor,
        Dictionary<string, CategorySortedAttribute> categorySortedByCategoryName)
    {
        var displayAttribute = propertyDescriptor.Attributes.OfType<DisplayAttribute>().FirstOrDefault();
        if (displayAttribute == null)
        {
            return new PropertyDescriptorExtended(propertyDescriptor);
        }
        var attributes = new List<Attribute>();
        var categoryName = displayAttribute.GetGroupName();
        if (!string.IsNullOrEmpty(categoryName))
        {
            if (categorySortedByCategoryName.TryGetValue(categoryName, out var categorySortedAttribute))
            {
                attributes.Add(categorySortedAttribute);

                // Replace any existing Category
                attributes.Add(new CategoryAttribute(categorySortedAttribute.Category));
            }
        }
        try
        {
            var displayName = displayAttribute.GetName();
            if (!string.IsNullOrEmpty(displayName))
            {
                var displayNameAttribute = new DisplayNameAttribute(displayAttribute.GetName());
                attributes.Add(displayNameAttribute);
            }
        }
        catch (Exception ex)
        {
            s_logger.Error(ex, "{Message}", ex.Message);
            throw;
        }
        var description = displayAttribute.GetDescription();
        if (!string.IsNullOrEmpty(description))
        {
            var descriptionAttribute = new DescriptionAttribute(displayAttribute.GetDescription());
            attributes.Add(descriptionAttribute);
        }
        var order = displayAttribute.GetOrder();
        if (order != null)
        {
            var orderAttribute = new PropertyOrderAttribute((int)order);
            attributes.Add(orderAttribute);
        }
        var result = new PropertyDescriptorExtended(propertyDescriptor);
        result = result.AddOrReplaceAttributes(attributes);
        return result;
    }

    /// <summary>
    /// Return the PropertyDescriptorExtended for name, or null if not found
    /// </summary>
    /// <param name="type">the type holding the property with name</param>
    /// <param name="name">the name of the property</param>
    /// <returns></returns>
    public static PropertyDescriptorExtended GetPropertyDescriptor(this Type type, string name)
    {
        var properties = TypeDescriptor.GetProperties(type);
        foreach (PropertyDescriptor property in properties)
        {
            if (property.Name == name)
            {
                return new PropertyDescriptorExtended(property);
            }
        }
        return null;
    }

    /// <summary>
    /// Return the attribute for a property with  for name, or null if not found
    /// </summary>
    /// <param name="type">the type holding the property with name</param>
    /// <param name="name">the name of the property</param>
    /// <param name="attributeType">the type of the attribute to return</param>
    /// <returns></returns>
    public static Attribute GetAttribute(this Type type, string name, Type attributeType)
    {
        var properties = TypeDescriptor.GetProperties(type);
        foreach (PropertyDescriptor property in properties)
        {
            if (property.Name == name)
            {
                foreach (Attribute propertyAttribute in property.Attributes)
                {
                    if (propertyAttribute.GetType() == attributeType)
                    {
                        return propertyAttribute;
                    }
                }
                break;
            }
        }
        return null;
    }

    public static PropertyDescriptorExtended[] ToExtendedArray(this PropertyDescriptorCollection propertyDescriptorCollection)
    {
        return ToList(propertyDescriptorCollection);
    }

    public static PropertyDescriptorExtended[] ToList(PropertyDescriptorCollection propertyDescriptorCollection)
    {
        var list = new List<PropertyDescriptorExtended>(propertyDescriptorCollection.Count);
        foreach (PropertyDescriptor pd in propertyDescriptorCollection)
        {
            list.Add(new PropertyDescriptorExtended(pd));
        }
        return list.ToArray();
    }

    public static PropertyDescriptorExtendedCollection ToExtendedCollection(this PropertyDescriptorCollection propertyDescriptorCollection)
    {
        return new PropertyDescriptorExtendedCollection(propertyDescriptorCollection.ToExtendedArray());
    }

    public static PropertyDescriptorExtendedCollection ToPropertyDescriptorExtendedCollection(
        this List<PropertyDescriptorExtended> propertyDescriptors)
    {
        PropertyDescriptor[] array = propertyDescriptors.ToArray();
        var result = new PropertyDescriptorExtendedCollection(array);
        return result;
    }

    public static PropertyDescriptorCollection RemoveNonBrowsable(this PropertyDescriptorCollection propertyDescriptorCollection)
    {
        var adjusted = new PropertyDescriptorCollection(null);
        for (var i = 0; i < propertyDescriptorCollection.Count; i++)
        {
            var pd = propertyDescriptorCollection[i];
            var attr = pd.Attributes.OfType<BrowsableAttribute>().FirstOrDefault();
            if (attr != null && !attr.Browsable)
            {
                continue;
            }
            adjusted.Add(pd);
        }
        return adjusted;
    }

    /// <summary>
    /// Return a list of PropertyDescriptorExtended after excluding Browsable(false) attributes,
    /// after converting Display attributes to ones that a WinForms PropertyGrid can handle,
    /// including support for CategoryOrder attributes on the class
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static List<PropertyDescriptorExtended> GetBrowsableWinformsPropertyDescriptors(this Type type)
    {
        var categorySortedByName = type.GetCategorySortedAttributesDictionary();
        var properties = TypeDescriptor.GetProperties(type);
        var result = new List<PropertyDescriptorExtended>(properties.Count);
        foreach (PropertyDescriptor property in properties)
        {
            var attr = property.Attributes.OfType<BrowsableAttribute>().FirstOrDefault();
            if (attr != null && !attr.Browsable)
            {
                continue;
            }
            var adjusted = property.ConvertDisplayAttributeForWinForms(categorySortedByName);
            result.Add(adjusted);
        }
        return result;
    }

    /// <summary>
    /// Thanks to https://stackoverflow.com/questions/1749966/c-sharp-how-to-determine-whether-a-type-is-a-number
    /// </summary>
    /// <param name="o"></param>
    /// <returns></returns>
    public static bool IsNumericType(this object o)
    {
        switch (Type.GetTypeCode(o.GetType()))
        {
            case TypeCode.Byte:
            case TypeCode.SByte:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
            case TypeCode.Decimal:
            case TypeCode.Double:
            case TypeCode.Single:
                return true;
            default:
                return false;
        }
    }
}