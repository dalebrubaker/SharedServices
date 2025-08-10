using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BruSoftware.SharedServices;
using BruSoftware.SharedServices.Attributes;
using BruSoftware.SharedServices.ExtensionMethods;
using FluentAssertions;
using NLog;
using Xunit;

namespace BruSoftware.SharedServicesTests;

// Ignore xUnit1031 because the Mutex blocking has thread affinity. I don't know another way to do this
[SuppressMessage("Usage", "xUnit1031:Do not use blocking task operations in test method")]
public class MiscellaneousTests
{
    private static readonly Logger s_logger = LogManager.GetCurrentClassLogger();

    [Fact]
    public void MutexProtectorBlocksTest()
    {
        const string name1 = "MutexProtectorBlocksTestString";
        var mutex2Blocked = true;
        using var mutex1 = new MutexProtector(name1);
        var exists = Utilities.MutexNameExists(name1);
        Assert.True(exists);
        var t = Task.Run(() =>
        {
            // Note this won't block if it is called on the owner's thread
            using (var mutex2 = new MutexProtector(name1))
            {
                mutex2Blocked = false;
            }
        });
        const int timeout = 100;
        t.Wait(timeout);
        Thread.Sleep(timeout);
        Assert.True(mutex2Blocked);
    }

    [Fact]
    public void MutexProtectorSecondFollowsFirstTest()
    {
        try
        {
            s_logger.ConditionalDebug("Starting MutexProtectorSecondFollowsFirstTest");
            const string name1 = "MutexProtectorSecondFollowsFirstTestName";
            const int timeout = 500;
            var mutex2Blocked = true;
            var sw = Stopwatch.StartNew();
            using (var mutex1 = new MutexProtector(name1))
            {
                var exists = Utilities.MutexNameExists(name1);
                Assert.True(exists);
                var t = Task.Run(() =>
                {
                    // Note this won't block if it is called on the owner's thread
                    using (var mutex2 = new MutexProtector(name1))
                    {
                        // Now that mutex1 is disposed, mutex2 should be unblocked
                        mutex2Blocked = false;
                        Assert.False(mutex2Blocked);
                    }
                });
                t.Wait(timeout);
                var elapsed = sw.ElapsedMilliseconds;
                sw.Stop();
                Assert.True(mutex2Blocked, $"mutex2Blocked={mutex2Blocked}");
                var msg = $"elapsed={elapsed} timeout={timeout}";
                s_logger.ConditionalDebug(msg);
                Assert.True(elapsed >= timeout - 15, msg);
                Thread.Sleep(100);
            }
            s_logger.ConditionalDebug("Finished MutexProtectorSecondFollowsFirstTest");
        }
        catch (Exception ex)
        {
            s_logger.Error(ex, "{Message}", ex.Message);
            s_logger.Error("{StackTrace}", Environment.StackTrace);
            throw new SharedServicesException("Why?");
            throw;
        }
    }

    [Fact]
    public void AddOrReplaceAttributeTests()
    {
        var testClass = new TestAttributes();
        var properties = testClass.GetType().GetBrowsablePropertyDescriptors();
        Assert.Equal(3, properties.Count);

        var propTestString = properties.First(x => x.Name == "TestString");
        Assert.NotNull(propTestString);

        // Test adding 2 new attributes
        var count = propTestString.Attributes.Count;
        var newPropTestString1 = propTestString.AddOrReplaceAttributes(new List<Attribute>
        {
            new DisplayNameAttribute("TestStringDisplayName"), new CategorySortedAttribute("TestCategory", 1, 8)
        });
        Assert.Equal(count + 2, newPropTestString1.Attributes.Count);

        // Test replacing two attributes
        var newPropTestString2 = propTestString.AddOrReplaceAttributes(new List<Attribute>
        {
            new DisplayNameAttribute("TestStringDisplayName2"), new CategorySortedAttribute("TestCategory2", 1, 8)
        });
        Assert.Equal(count + 2, newPropTestString2.Attributes.Count);
        Assert.Equal("TestStringDisplayName2", newPropTestString2.DisplayName);

        // Test
        var categorySortedByName = testClass.GetType().GetCategorySortedAttributesDictionary();
        Assert.Equal(2, categorySortedByName.Count);
    }

    [Fact]
    public void GetCategorySortedAttributesDictionaryTests()
    {
        var testClass = new TestAttributes();
        var categorySortedByName = testClass.GetType().GetCategorySortedAttributesDictionary();
        Assert.Equal(2, categorySortedByName.Count);
    }

    [Fact]
    public void ConvertDisplayAttributeForWinFormsTests()
    {
        var testClass = new TestAttributes();
        var propertyDescriptors = testClass.GetType().GetPropertyDescriptorsWithAttribute(new DisplayAttribute());
        Assert.Equal(3, propertyDescriptors.Count);
        var pd = propertyDescriptors[0];
        Assert.Equal("TestString", pd.Name);

        var categorySortedByName = testClass.GetType().GetCategorySortedAttributesDictionary();
        Assert.Equal(2, categorySortedByName.Count);
        var pd1 = pd.ConvertDisplayAttributeForWinForms(categorySortedByName);
        Assert.Equal("TestStringDescription", pd1.Description);
        Assert.EndsWith("String Category", pd1.Category); // will have 1 leading tab
    }

    [Fact]
    public void CircularBufferTests()
    {
        var buffer = new CircularBuffer<int>(3);
        buffer.Add(1);
        buffer.Add(2);
        buffer.Add(3);
        var peek0 = buffer.Peek();
        Assert.Equal(1, peek0);
        var peekTail0 = buffer.PeekTail();
        Assert.Equal(3, peekTail0);

        buffer.Add(4);
        var value0 = buffer[0];
        Assert.Equal(4, value0);
        var peek1 = buffer.Peek();
        Assert.Equal(2, peek1);
        var peekTail1 = buffer.PeekTail();
        Assert.Equal(4, peekTail1);
    }

    [Fact]
    public void PermutationsTests()
    {
        var testClass = new TestClass();
        var properties = typeof(TestClass).GetBrowsablePropertyDescriptors().Cast<PropertyDescriptor>().ToList();
        var propertyValuesByPropertyName = new Dictionary<string, List<object>>
        {
            {
                "MyString", new List<object>
                {
                    "One", "Two", "Three"
                }
            }

            //{"MyInt", new List<object> {1, 2, 3}},
            //{"MyDouble", new List<object> {1.1, 2.2, 3.3}},
        };
        var counter = 0;
        foreach (var permutation in Permutations<TestClass>.GetPermutations(testClass, propertyValuesByPropertyName, properties))
        {
            counter++;
        }
        Assert.Equal(3, counter);

        propertyValuesByPropertyName["MyInt"] = new List<object>
        {
            1, 2, 3
        };
        var counter2 = 0;
        foreach (var permutation in Permutations<TestClass>.GetPermutations(testClass, propertyValuesByPropertyName, properties))
        {
            counter2++;
        }
        Assert.Equal(9, counter2);

        propertyValuesByPropertyName["MyDouble"] = new List<object>
        {
            1.1, 2.2, 3.3
        };
        var counter3 = 0;
        foreach (var permutation in Permutations<TestClass>.GetPermutations(testClass, propertyValuesByPropertyName, properties))
        {
            counter3++;
        }
        Assert.Equal(27, counter3);
    }

    [Fact]
    public void OpenUniqueFileNameSequentialTests()
    {
        var result0 = Utilities.OpenUniqueFileNameSequential("", "Prefix", ".tmp", FileMode.CreateNew);
        Assert.NotNull(result0);
        var result1 = Utilities.OpenUniqueFileNameSequential("", "Prefix", ".tmp", FileMode.CreateNew);
        Assert.NotNull(result1);
    }

    /// <summary>
    /// See example at https://en.cppreference.com/w/cpp/algorithm/lower_bound
    /// </summary>
    [Fact]
    public void GetLowerBound_ShouldWork()
    {
        var list = new List<int> { 100, 200, 300 };
        var lowerBound = list.LowerBound(0);
        lowerBound.Should().Be(0);
        lowerBound = list.LowerBound(100);
        lowerBound.Should().Be(0);
        lowerBound = list.LowerBound(150);
        lowerBound.Should()
            .Be(1); // Returns an iterator pointing to the first element in the range [first,last) which does not compare less than val.
        lowerBound = list.LowerBound(200);
        lowerBound.Should().Be(1);
        lowerBound = list.LowerBound(300);
        lowerBound.Should().Be(2);
        lowerBound = list.LowerBound(400);
        lowerBound.Should().Be(3);
    }
}