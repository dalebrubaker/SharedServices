using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using BruSoftware.SharedServices.Attributes;
using BruSoftware.SharedServices.Converters;
using Newtonsoft.Json;

namespace BruSoftware.SharedServices;

public class PropertyDescriptorExtended : PropertyDescriptor
{
    private readonly string _displayName;
    private readonly PropertyDescriptor _innerPropertyDescriptor;
    private readonly string _name;
    private readonly Func<object, object> _objectDelegate;

    [JsonConstructor]
    public PropertyDescriptorExtended(PropertyDescriptor descr) : base(descr)
    {
        _innerPropertyDescriptor = descr;
    }

    public PropertyDescriptorExtended(PropertyDescriptor descr, Attribute[] attrs) : base(descr, attrs)
    {
        _innerPropertyDescriptor = descr;
    }

    public PropertyDescriptorExtended(PropertyDescriptor propertyDescriptor, Func<object, object> objectDelegate, string displayName) : this(
        propertyDescriptor)
    {
        _objectDelegate = objectDelegate;
        _displayName = displayName;
    }

    public PropertyDescriptorExtended(PropertyDescriptor propertyDescriptor, Func<object, object> objectDelegate, string name, string displayName,
        Attribute[] attributes) : this(propertyDescriptor, attributes)
    {
        _objectDelegate = objectDelegate;
        _name = name;
        _displayName = displayName;
    }

    public PropertyDescriptorExtended(PropertyDescriptor propertyDescriptor, Func<object, object> objectDelegate, string displayName,
        Attribute[] attributes) : this(propertyDescriptor, attributes)
    {
        _objectDelegate = objectDelegate;
        _displayName = displayName;
    }

    public override string DisplayName => _displayName ?? base.DisplayName;

    public override Type ComponentType => _innerPropertyDescriptor.ComponentType;
    public override bool IsReadOnly => _innerPropertyDescriptor.IsReadOnly;
    public override Type PropertyType => _innerPropertyDescriptor.PropertyType;

    /// <summary>
    /// Construct and add optional parameters. Existing attributes will NOT be replaced.
    /// </summary>
    /// <param name="category">Won't be added if null</param>
    /// <param name="displayName">Won't be added if null</param>
    /// <param name="description">Won't be added if null</param>
    /// <param name="defaultValue">Won't be added if null</param>
    /// <param name="propertyOrder">Won't be added if -1</param>
    /// <param name="typeConverterType"></param>
    /// <param name="categorySorted"></param>
    /// <param name="setToNotBrowsable"></param>
    /// <returns>a custom property descriptor with the changes</returns>
    public PropertyDescriptorExtended AddOrReplaceAttributes(string category = null, string displayName = null,
        string description = null, object defaultValue = null, int propertyOrder = -1,
        Type typeConverterType = null, CategorySortedAttribute categorySorted = null, bool setToNotBrowsable = false)
    {
        var newProperty = this;
        if (!string.IsNullOrEmpty(category))
        {
            newProperty = newProperty.AddOrReplaceAttribute(new CategoryAttribute(category));
        }
        if (!string.IsNullOrEmpty(displayName))
        {
            newProperty = newProperty.AddOrReplaceAttribute(new DisplayNameAttribute(displayName));
        }
        if (!string.IsNullOrEmpty(description))
        {
            newProperty = newProperty.AddOrReplaceAttribute(new DescriptionAttribute(description));
        }
        if (defaultValue != null)
        {
            newProperty = newProperty.AddOrReplaceAttribute(new DefaultValueAttribute(defaultValue));
        }
        if (propertyOrder >= 0)
        {
            newProperty = newProperty.AddOrReplaceAttribute(new PropertyOrderAttribute(propertyOrder));
        }
        if (typeConverterType != null)
        {
            newProperty = newProperty.AddOrReplaceAttribute(new TypeConverterAttribute(typeConverterType));
        }
        if (categorySorted != null)
        {
            newProperty = newProperty.AddOrReplaceAttribute(categorySorted);
        }
        if (setToNotBrowsable)
        {
            newProperty = newProperty.AddOrReplaceAttribute(new BrowsableAttribute(false));
        }
        return newProperty;
    }

    /// <summary>
    /// Add or replace attributes in this property and return a new one
    /// </summary>
    /// <param name="attribute"></param>
    /// <returns></returns>
    public PropertyDescriptorExtended AddOrReplaceAttribute(Attribute attribute)
    {
        var copy = new Attribute[Attributes.Count];
        Attributes.CopyTo(copy, 0);
        var newAttributes = copy.ToList();
        var index = newAttributes.FindIndex(x => x.TypeId.ToString().Contains(attribute.ToString()));
        if (index >= 0)
        {
            newAttributes.RemoveAt(index);
        }
        newAttributes.Add(attribute);
        return new PropertyDescriptorExtended(this, newAttributes.ToArray());
    }

    /// <summary>
    /// Add or replace attributes in this property and return a new one
    /// </summary>
    /// <param name="attributes"></param>
    /// <returns></returns>
    public PropertyDescriptorExtended AddOrReplaceAttributes(IList<Attribute> attributes)
    {
        var newList = new List<Attribute>(Attributes.Count + attributes.Count);
        foreach (Attribute oldAttr in Attributes)
        {
            newList.Add(oldAttr);
        }
        foreach (var newAttr in attributes)
        {
            // Remove any existing attributes of the same type
            newList.RemoveAll(x => x.GetType() == newAttr.GetType());
            newList.Add(newAttr);
        }
        var result = new PropertyDescriptorExtended(this, newList.ToArray());
        return result;
    }

    public override bool CanResetValue(object component)
    {
        return _innerPropertyDescriptor.CanResetValue(component);
    }

    public override object GetValue(object component)
    {
        return _innerPropertyDescriptor.GetValue(_objectDelegate == null ? component : _objectDelegate(component));
    }

    public override void ResetValue(object component)
    {
        _innerPropertyDescriptor.ResetValue(_objectDelegate == null ? component : _objectDelegate(component));
    }

    public override void SetValue(object component, object value)
    {
        value = RequireInRange(value);
        _innerPropertyDescriptor.SetValue(_objectDelegate == null ? component : _objectDelegate(component), value);
    }

    private object RequireInRange(object value)
    {
        var attr = Attributes.OfType<RangeAttribute>().FirstOrDefault();
        if (attr == null)
        {
            return value;
        }
        var valueT = Convert.ChangeType(value, attr.Minimum.GetType());
        if (valueT is IComparable val)
        {
            if (val.CompareTo(attr.Minimum) < 0)
            {
                return attr.Minimum;
            }
            if (val.CompareTo(attr.Maximum) > 0)
            {
                return attr.Maximum;
            }
            return value;
        }
        return value;
    }

    public override bool ShouldSerializeValue(object component)
    {
        return _innerPropertyDescriptor.ShouldSerializeValue(_objectDelegate == null ? component : _objectDelegate(component));
    }

    public bool HasAttribute(Attribute attribute)
    {
        foreach (Attribute attr in Attributes)
        {
            if (attr.TypeId == attribute.TypeId)
            {
                return true;
            }
        }
        return false;
    }

    public override string ToString()
    {
        return DisplayName;
    }
}