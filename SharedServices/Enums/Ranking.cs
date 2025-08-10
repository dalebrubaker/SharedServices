using BruSoftware.SharedServices.Attributes;

namespace BruSoftware.SharedServices;

public enum Ranking
{
    [Abbreviation("Dec")] Decile,

    [Abbreviation("Prc")] Percentile,

    [Abbreviation("Mil")] Millile
}