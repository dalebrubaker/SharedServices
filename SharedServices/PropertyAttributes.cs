using System;

namespace BruSoftware.SharedServices;

public class PropertyAttributes
{
    public PropertyAttributes(string name, Attribute[] attributes)
    {
        Name = name;
        Attributes = attributes;
    }

    public string Name { get; }

    public Attribute[] Attributes { get; }
}