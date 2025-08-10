using System;

namespace BruSoftware.SharedServices.Attributes;

[AttributeUsage(AttributeTargets.All)]
public class AbbreviationAttribute : Attribute
{
    public AbbreviationAttribute(string abbrev)
    {
        Abbrev = abbrev;
    }

    public string Abbrev { get; }
}