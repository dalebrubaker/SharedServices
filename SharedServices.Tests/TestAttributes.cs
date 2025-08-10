using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using BruSoftware.SharedServices.Attributes;

namespace BruSoftware.SharedServicesTests;

[CategoryOrder("Int Category", 10)]
[CategoryOrder("String Category", 20)]
internal class TestAttributes
{
    [Display(Description = "TestStringDescription", Name = "TestString1Name", Order = 0, GroupName = "String Category")]
    public string TestString { get; set; } = "";

    [Range(1, int.MaxValue)]
    [Display(Description = "TestInt1Description", Name = "TestInt1Name", Order = 1, GroupName = "Int Category")]
    public int TestInt1 { get; set; }

    [Browsable(false)]
    public int TestNotBrowsableInt { get; set; }

    [Browsable(true)]
    [Range(1, int.MaxValue)]
    [Display(Description = "TestInt0Description", Name = "TestInt0Name", Order = 0, GroupName = "Int Category")]
    public int TestInt0 { get; set; }
}