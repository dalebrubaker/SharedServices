using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BruSoftware.SharedServices;

public static class Permutations<T> where T : class
{
    /// <summary>
    /// Warning: Each permutation is the same instance. Clone it if you need to keep them separate, e.g. ToList()
    /// </summary>
    /// <param name="myClass"></param>
    /// <param name="propertyValuesByPropertyName"></param>
    /// <param name="properties"></param>
    /// <returns></returns>
    public static IEnumerable<T> GetPermutations(T myClass, Dictionary<string, List<object>> propertyValuesByPropertyName,
        List<PropertyDescriptor> properties)
    {
        if (propertyValuesByPropertyName.Count <= 0)
        {
            yield return myClass;
            yield break;
        }
        var propertyNames = propertyValuesByPropertyName.Keys.ToList();
        foreach (var result in GetPermutations(myClass, 0, propertyNames, propertyValuesByPropertyName, properties))
        {
            yield return result;
        }
    }

    private static IEnumerable<T> GetPermutations(T myClass, int level, List<string> propertyNames,
        Dictionary<string, List<object>> propertyValuesByPropertyName,
        List<PropertyDescriptor> properties)
    {
        var propertyName = propertyNames[level];
        var values = propertyValuesByPropertyName[propertyName];
        foreach (var value in values)
        {
            var property = properties.FirstOrDefault(x => x.Name == propertyName || x.DisplayName == propertyName);
            if (property == null)
            {
                throw new ArgumentException($"Invalid propertyName {propertyName}");
            }
            property.SetValue(myClass, value);
            if (level == propertyNames.Count - 1)
            {
                // We're at the highest level, so this permutation of myClass is finished
                yield return myClass;
            }
            else
            {
                // Set permutations for all higher levels, recursively
                foreach (var result in GetPermutations(myClass, level + 1, propertyNames, propertyValuesByPropertyName, properties))
                {
                    yield return result;
                }
            }
        }
    }

    public static IEnumerable<List<T>> GetPermutations(List<List<T>> allLists, int level, List<T> resultSoFar)
    {
        if (allLists.Count == 0)
        {
            yield return resultSoFar;
            yield break;
        }
        var list = allLists[level];
        foreach (var value in list)
        {
            resultSoFar[level] = value;
            if (level == allLists.Count - 1)
            {
                // We're at the highest level, so this permutation is finished
                yield return new List<T>(resultSoFar);
            }
            else
            {
                // Set permutations for all higher levels, recursively
                foreach (var result in GetPermutations(allLists, level + 1, resultSoFar))
                {
                    yield return result;
                }
            }
        }
    }
}