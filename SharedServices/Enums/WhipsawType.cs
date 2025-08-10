using BruSoftware.SharedServices.Attributes;

namespace BruSoftware.SharedServices;

public enum WhipsawType
{
    // Class WhipsawPrctPrct
    [Abbreviation("I%%S")]
    IntervalPrctPrctScale,

    [Abbreviation("I%%RS")]
    IntervalPrctPrctReversingNoScale
}