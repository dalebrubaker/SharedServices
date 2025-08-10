using System;
using BruSoftware.SharedServices.Attributes;

namespace BruSoftware.SharedServices.ExtensionMethods;

public static class EnumExtensions
{
    public static string GetAbbreviationAttributeValue<T>(this T @enum)
    {
        var attributeValue = string.Empty;
        if (@enum != null)
        {
            var fi = @enum.GetType().GetField(@enum.ToString());
            if (fi != null)
            {
                var attributes = fi.GetCustomAttributes(typeof(AbbreviationAttribute), false);
                if (attributes.Length > 0 && attributes[0] is AbbreviationAttribute attribute)
                {
                    attributeValue = attribute.Abbrev;
                }
            }
        }
        return attributeValue;
    }

    public static int ToMultiplier(this Ranking ranking)
    {
        switch (ranking)
        {
            case Ranking.Decile:
                return 10;
            case Ranking.Percentile:
                return 100;
            case Ranking.Millile:
                return 1000;
            default:
                throw new ArgumentOutOfRangeException(nameof(ranking), ranking, null);
        }
    }
}