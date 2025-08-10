using System;

namespace BruSoftware.SharedServices.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class CategoryOrderAttribute : Attribute
{
    private const char NonPrintableChar = '\t';

    public CategoryOrderAttribute(string category, int order)
    {
        Order = order;
        Category = category;
    }

    public CategoryOrderAttribute(Type resourceType, string category, int order) : this(category, order)
    {
        ResourceType = resourceType;
    }

    public string Category { get; }

    public int Order { get; }

    public Type ResourceType { get; }

    public override string ToString()
    {
        return $"{Category}, Order={Order}";
    }
}