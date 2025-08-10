namespace BruSoftware.SharedServicesTests;

internal class TestClass
{
    public string MyString { get; set; } = "";
    public int MyInt { get; set; }
    public double MyDouble { get; set; }

    public override string ToString()
    {
        return $"{MyString}, {MyInt}, {MyDouble}";
    }
}