using System;
using System.Collections;
using System.ComponentModel;
using NLog;

namespace BruSoftware.SharedServices.Converters;

/// <summary>
/// Thanks to http://www.codeproject.com/Articles/6611/Ordering-Items-in-the-Property-Grid
/// Note that sub-sorting is done within each Category
/// </summary>
public class PropertySorter : ExpandableObjectConverter
{
    private static readonly Logger s_logger = LogManager.GetCurrentClassLogger();

    #region Methods

    public override bool GetPropertiesSupported(ITypeDescriptorContext context)
    {
        return true;
    }

    public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
    {
        // Added this so Json.Net would work
        return destinationType != typeof(string);
    }

    public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
    {
        //
        // This override returns a list of properties in order
        //
        try
        {
            var pdc = TypeDescriptor.GetProperties(value, attributes);
            return SortProperties(pdc);
        }
        catch (Exception ex)
        {
            s_logger.Error(ex, "{Message}", ex.Message);
            throw;
        }
    }

    protected static PropertyDescriptorCollection SortProperties(PropertyDescriptorExtendedCollection pdc)
    {
        try
        {
            var orderedProperties = new ArrayList();
            foreach (PropertyDescriptor pd in pdc)
            {
                var attribute = pd.Attributes[typeof(PropertyOrderAttribute)];
                if (attribute != null)
                {
                    //
                    // If the attribute is found, then create an pair object to hold it
                    //
                    var poa = (PropertyOrderAttribute)attribute;
                    orderedProperties.Add(new PropertyOrderPair(pd.Name, poa.Order));
                }
                else
                {
                    //
                    // If no order attribute is specified then given it an order of 0
                    //
                    orderedProperties.Add(new PropertyOrderPair(pd.Name, 0));
                }
            }

            //
            // Perform the actual order using the value PropertyOrderPair classes
            // implementation of IComparable to sort
            //
            orderedProperties.Sort();

            //
            // Build a string list of the ordered names
            //
            var propertyNames = new ArrayList();
            foreach (PropertyOrderPair pop in orderedProperties)
            {
                propertyNames.Add(pop.Name);
            }

            //
            // Pass in the ordered list for the PropertyDescriptorCollection to sort by
            //
            var result = pdc.Sort((string[])propertyNames.ToArray(typeof(string)));
            return result;
        }
        catch (Exception ex)
        {
            s_logger.Error(ex, "{Message}", ex.Message);
            throw;
        }
    }

    protected static PropertyDescriptorCollection SortProperties(PropertyDescriptorCollection pdc)
    {
        var extended = pdc.ToExtendedCollection();
        var result = SortProperties(extended);
        return result;
    }

    #endregion Methods
}

#region Helper Class - PropertyOrderAttribute

[AttributeUsage(AttributeTargets.Property)]
public class PropertyOrderAttribute : Attribute
{
    //
    // Simple attribute to allow the order of a property to be specified
    //
    public PropertyOrderAttribute(int order)
    {
        Order = order;
    }

    public int Order { get; }
}

#endregion Helper Class - PropertyOrderAttribute

#region Helper Class - PropertyOrderPair

public class PropertyOrderPair : IComparable
{
    private readonly int _order;

    public PropertyOrderPair(string name, int order)
    {
        _order = order;
        Name = name;
    }

    public string Name { get; }

    public int CompareTo(object obj)
    {
        //
        // Sort the pair objects by ordering by order value
        // Equal values get the same rank
        //
        var otherOrder = ((PropertyOrderPair)obj)._order;
        if (otherOrder == _order)
        {
            //
            // If order not specified, sort by name
            //
            var otherName = ((PropertyOrderPair)obj).Name;
            return string.CompareOrdinal(Name, otherName);
        }
        if (otherOrder > _order)
        {
            return -1;
        }
        return 1;
    }
}

#endregion Helper Class - PropertyOrderPair