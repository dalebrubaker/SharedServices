using System.Runtime.CompilerServices;

namespace BruSoftware.SharedServices;

/// <summary>
/// Handle doubles precision comparison of floats
/// </summary>
public static class FloatExtensions
{
    //        private static bool _IsDoubleMachinePrecisionSet;
    //        // ReSharper disable once InconsistentNaming
    //        public static double _doubleMachinePrecision;

    //        /// <summary>
    //        /// Calculate Epsilon for a double.
    //        /// Thanks to http://stackoverflow.com/questions/9392869/where-do-i-find-the-machine-epsilon-in-c
    //        /// </summary>
    //        public static double DoubleMachinePrecision
    //        {
    //            get
    //            {
    //                if (!_IsDoubleMachinePrecisionSet)
    //                {
    //                    _doubleMachinePrecision = 1.0d;

    //                    do
    //                    {
    //                        _doubleMachinePrecision /= 2.0d;
    //#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
    //                        // ReSharper disable once CompareOfFloatsByEqualityOperator
    //                    } while ((1.0 + _doubleMachinePrecision) != 1.0);
    //#pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator
    //                    _IsDoubleMachinePrecisionSet = true;
    //                }
    //                return _doubleMachinePrecision;
    //            }
    //        }

    /// <summary>
    /// From CoPilot
    /// </summary>
    /// <returns></returns>
    private static double CalculateMachineEpsilon()
    {
        var epsilon = 1.0;
        while (1.0 + epsilon / 2.0 != 1.0)
        {
            epsilon /= 2.0;
        }
        return epsilon;
    }

    /// <summary>
    /// Return true for value1 equal to value2 using the c++ approach not(x less than y) and not(y less than x)
    /// </summary>
    /// <param name="value1"></param>
    /// <param name="value2"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsEqual(this float value1, float value2)
    {
        if (value1 < value2)
        {
            return false;
        }
        if (value2 < value1)
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// Return true for value1 equal to value2 using the c++ approach not(x less than y) and not(y less than x)
    /// </summary>
    /// <param name="value1"></param>
    /// <param name="value2"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNotEqual(this float value1, float value2)
    {
        if (value1 < value2)
        {
            return true;
        }
        if (value2 < value1)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Return true for value equal to 0.0 using the c++ approach not(x less than y) and not(y less than x)
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool IsZero(this float value)
    {
        if (value < 0)
        {
            return false;
        }
        if (0 < value)
        {
            return false;
        }
        return true;
    }

    public static bool IsNaN(this float value)
    {
        var result = float.IsNaN(value);
        return result;
    }

    /// <summary>
    /// Return true for value equal to double.MinValue
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool IsMinValue(this float value)
    {
        if (value > float.MinValue)
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// Return true for value equal to double.MaxValue
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool IsMaxValue(this float value)
    {
        if (value < float.MaxValue)
        {
            return false;
        }
        return true;
    }
}