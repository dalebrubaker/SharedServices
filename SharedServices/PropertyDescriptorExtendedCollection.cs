using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace BruSoftware.SharedServices;

/// <summary>
/// This class throws CollectionChangedEvent when the collection is changed, but NOT when it is sorted.
/// </summary>
public class PropertyDescriptorExtendedCollection : PropertyDescriptorCollection
{
    public PropertyDescriptorExtendedCollection(PropertyDescriptor[] properties) : base(properties)
    {
    }

    public PropertyDescriptorExtendedCollection(PropertyDescriptor[] properties, bool readOnly) : base(properties, readOnly)
    {
    }

    public new int Add(PropertyDescriptor value)
    {
        var result = base.Add(value);
        OnCollectionChanged();
        return result;
    }

    public new void Insert(int index, PropertyDescriptor value)
    {
        base.Insert(index, value);
        OnCollectionChanged();
    }

    public new void Clear()
    {
        base.Clear();
        OnCollectionChanged();
    }

    public new void Remove(PropertyDescriptor value)
    {
        base.Remove(value);
        OnCollectionChanged();
    }

    public new void RemoveAt(int index)
    {
        base.RemoveAt(index);
        OnCollectionChanged();
    }

    public PropertyDescriptor[] ToArray()
    {
        var result = new PropertyDescriptor[Count];
        for (var i = 0; i < Count; i++)
        {
            result[i] = this[i];
        }
        return result;
    }

    public List<PropertyDescriptor> ToList()
    {
        var result = new List<PropertyDescriptor>(Count);
        for (var i = 0; i < Count; i++)
        {
            result.Add(this[i]);
        }
        return result;
    }

    /// <summary>
    /// Same as ToList() but returns PropertyDescriptorExtended instead of PropertyDescriptor
    /// </summary>
    /// <returns></returns>
    public IList<PropertyDescriptorExtended> ToCustomList()
    {
        var result = new List<PropertyDescriptorExtended>(Count);
        for (var i = 0; i < Count; i++)
        {
            result.Add(new PropertyDescriptorExtended(this[i]));
        }
        return result;
    }

    public PropertyDescriptorExtendedCollection Clone()
    {
        var list = ToList();
        var result = new PropertyDescriptorExtendedCollection(ToArray(), false);
        return result;
    }

    public event EventHandler CollectionChangedEvent;

    private void OnCollectionChanged()
    {
        var temp = CollectionChangedEvent; // for thread safety
        temp?.Invoke(this, EventArgs.Empty);
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        for (var i = 0; i < Count; i++)
        {
            sb.Append(this[i].Name);
            sb.Append(',');
        }
        if (Count > 0)
        {
            sb.Length--;
        }
        return sb.ToString();
    }
}