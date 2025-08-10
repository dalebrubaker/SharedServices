using System;

namespace BruSoftware.SharedServices.Attributes;

/// <summary>
/// Thanks to
/// https://stackoverflow.com/questions/4086105/expand-c-sharp-propertygrid-on-show?utm_medium=organic&amp;utm_source=google_rich_qa&amp;utm_campaign=google_rich_qa
/// </summary>
[AttributeUsage(AttributeTargets.All)]
public class PropertyGridInitialExpandedAttribute : Attribute
{
    public PropertyGridInitialExpandedAttribute(bool initialExpanded)
    {
        InitialExpanded = initialExpanded;
    }

    public bool InitialExpanded { get; set; }
}