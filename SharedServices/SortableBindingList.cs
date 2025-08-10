// From https://referencesource.microsoft.com/#system.data.linq/SortableBindingList.cs but internal -> public

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Linq;

namespace BruSoftware.SharedServices;

/// <summary>
/// Adds sorting feature to BindingList
/// </summary>
/// <typeparam name="T">The type of elements in the list</typeparam>
public class SortableBindingList<T> : BindingList<T>
{
    private bool _isSorted;
    private ListSortDirection _sortDirection = ListSortDirection.Ascending;
    private PropertyDescriptor _sortProperty;

    public SortableBindingList(IList<T> list) : base(list)
    {
    }

    protected override ListSortDirection SortDirectionCore => _sortDirection;

    protected override PropertyDescriptor SortPropertyCore => _sortProperty;

    protected override bool IsSortedCore => _isSorted;

    protected override bool SupportsSortingCore => true;

    protected override void RemoveSortCore()
    {
        _isSorted = false;
        _sortProperty = null;
    }

    protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
    {
        //Only apply sort if the column is sortable, decision was made not to throw in this case.
        //Don't prevent nullable types from working.
        var propertyType = prop.PropertyType;

        if (PropertyComparer.IsAllowable(propertyType))
        {
            ((List<T>)Items).Sort(new PropertyComparer(prop, direction));
            _sortDirection = direction;
            _sortProperty = prop;
            _isSorted = true;
            OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }
    }

    internal class PropertyComparer : Comparer<T>
    {
        private readonly IComparer comparer;
        private readonly ListSortDirection direction;
        private readonly PropertyDescriptor prop;
        private readonly bool useToString;

        internal PropertyComparer(PropertyDescriptor prop, ListSortDirection direction)
        {
            if (prop.ComponentType != typeof(T))
            {
                throw new MissingMemberException(typeof(T).Name, prop.Name);
            }
            this.prop = prop;
            this.direction = direction;

            if (OkWithIComparable(prop.PropertyType))
            {
                var comparerType = typeof(Comparer<>).MakeGenericType(prop.PropertyType);
                var defaultComparer = comparerType.GetProperty("Default");
                comparer = (IComparer)defaultComparer.GetValue(null, null);
                useToString = false;
            }
            else if (OkWithToString(prop.PropertyType))
            {
                comparer = StringComparer.CurrentCultureIgnoreCase;
                useToString = true;
            }
        }

        public override int Compare(T x, T y)
        {
            var xValue = prop.GetValue(x);
            var yValue = prop.GetValue(y);

            if (useToString)
            {
                xValue = xValue != null ? xValue.ToString() : null;
                yValue = yValue != null ? yValue.ToString() : null;
            }

            if (direction == ListSortDirection.Ascending)
            {
                return comparer.Compare(xValue, yValue);
            }
            return comparer.Compare(yValue, xValue);
        }

        protected static bool OkWithToString(Type t)
        {
            // this is the list of types that behave specially for the purpose of
            // sorting. if we have a property of this type, and it does not implement
            // IComparable, then this class will sort the properties according to the
            // ToString() method.

            // In the case of an XNode, the ToString() returns the
            // XML, which is what we care about.
            return t.Equals(typeof(XNode)) || t.IsSubclassOf(typeof(XNode));
        }

        protected static bool OkWithIComparable(Type t)
        {
            return t.GetInterface("IComparable") != null
                   || t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static bool IsAllowable(Type t)
        {
            return OkWithToString(t) || OkWithIComparable(t);
        }
    }
}