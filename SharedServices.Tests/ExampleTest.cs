using Xunit;
using FluentAssertions;

namespace BruSoftware.SharedServices.Tests;

public class ExampleTest
{
    [Fact]
    public void Example_Test_Should_Pass()
    {
        var result = 1 + 1;
        result.Should().Be(2);
    }
}