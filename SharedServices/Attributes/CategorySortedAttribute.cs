using System.ComponentModel;

namespace BruSoftware.SharedServices.Attributes;

/// <summary>
/// Thanks to Joel B at https://stackoverflow.com/questions/823327/how-can-i-customize-category-sorting-on-a-propertygrid
/// </summary>
public class CategorySortedAttribute : CategoryAttribute
{
    private const char NonPrintableChar = '\t';
    private readonly int _categoryPos;
    private readonly int _totalCategories;

    public CategorySortedAttribute()
    {
    }

    public CategorySortedAttribute(string category, int categoryPos, int totalCategories)
        : base(category.PadLeft(category.Length + (totalCategories - categoryPos), NonPrintableChar))
    {
        _categoryPos = categoryPos;
        _totalCategories = totalCategories;
    }

    public override string ToString()
    {
        return $"{Category}, {_categoryPos} of {_totalCategories}";
    }
}