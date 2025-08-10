using System;
using System.Collections.Generic;
using System.Linq;

namespace BruSoftware.SharedServices;

/// <summary>
/// From https://stackoverflow.com/questions/5179866/xirr-calculation/5185144 by Gonzalo Contento
/// </summary>
public static class XIRR
{
    private const double DaysPerYear = 365.0;
    private const int MaxIterations = 100;
    private const double DefaultTolerance = 1E-6;
    private const double DefaultGuess = 0.1;

    public static double CalcXIRR(IList<CashItem> cashFlow, double guess = DefaultGuess, double tolerance = DefaultTolerance,
        int maxIterations = MaxIterations)
    {
        if (cashFlow.Count(cf => cf.Amount > 0) == 0)
        {
            throw new XIRRArgumentException("Add at least one positive item");
        }

        if (cashFlow.Count(c => c.Amount < 0) == 0)
        {
            throw new XIRRArgumentException("Add at least one negative item");
        }

        var (result, success) = NewtonsMethodImplementation(cashFlow, guess, tolerance, maxIterations);
        if (!success)
        {
            result = BisectionMethodImplementation(cashFlow, tolerance, maxIterations);
        }
        if (double.IsInfinity(result))
        {
            throw new XIRRException("Could not calculate: Infinity");
        }

        if (double.IsNaN(result))
        {
            throw new XIRRException("Could not calculate: Not a number");
        }

        return result;
    }

    private static (double IRR, bool success) NewtonsMethodImplementation(IList<CashItem> cashFlow, double guess = DefaultGuess,
        double tolerance = DefaultTolerance, int maxIterations = MaxIterations)
    {
        var x0 = guess;
        var i = 0;
        double error;
        do
        {
            var dfx0 = XnpvPrime(cashFlow, x0);
            if (Math.Abs(dfx0 - 0) < double.Epsilon)
            {
                return (double.NaN, false);
                //throw new XIRRException("Could not calculate: No solution found. df(x) = 0");
            }

            var fx0 = Xnpv(cashFlow, x0);
            var x1 = x0 - fx0 / dfx0;
            error = Math.Abs(x1 - x0);

            x0 = x1;
        } while (error > tolerance && ++i < maxIterations);

        if (i == maxIterations)
        {
            return (double.NaN, false);
            //throw new XIRRException("Could not calculate: No solution found. Max iterations reached.");
        }

        var success = !double.IsNaN(x0);
        return (x0, success);
    }

    private static double BisectionMethodImplementation(IList<CashItem> cashFlow, double tolerance = DefaultTolerance,
        int maxIterations = MaxIterations)
    {
        // From "Applied Numerical Analysis" by Gerald
        var brackets = Brackets.Find(Xnpv, cashFlow);
        if (Math.Abs(brackets.First - brackets.Second) < double.Epsilon)
        {
            throw new XIRRArgumentException("Could not calculate: bracket failed");
        }

        double f3;
        double result;
        var x1 = brackets.First;
        var x2 = brackets.Second;

        var i = 0;
        do
        {
            var f1 = Xnpv(cashFlow, x1);
            var f2 = Xnpv(cashFlow, x2);

            if (Math.Abs(f1) < double.Epsilon && Math.Abs(f2) < double.Epsilon)
            {
                throw new XIRRException("Could not calculate: No solution found");
            }

            if (f1 * f2 > 0)
            {
                throw new XIRRArgumentException("Could not calculate: bracket failed for x1, x2");
            }

            result = (x1 + x2) / 2;
            f3 = Xnpv(cashFlow, result);

            if (f3 * f1 < 0)
            {
                x2 = result;
            }
            else
            {
                x1 = result;
            }
        } while (Math.Abs(x1 - x2) / 2 > tolerance && Math.Abs(f3) > double.Epsilon && ++i < maxIterations);

        if (i == maxIterations)
        {
            throw new XIRRException("Could not calculate: No solution found");
        }

        return result;
    }

    private static double Xnpv(IList<CashItem> cashFlow, double rate)
    {
        if (rate <= -1)
        {
            rate = -1 + 1E-10; // Very funky ... Better check what an IRR <= -100% means
        }

        var startDate = cashFlow.OrderBy(i => i.Date).First().Date;
        return (from item in cashFlow let days = -(item.Date - startDate).Days select item.Amount * Math.Pow(1 + rate, days / DaysPerYear)).Sum();
    }

    private static double XnpvPrime(IList<CashItem> cashFlow, double rate)
    {
        var startDate = cashFlow.OrderBy(i => i.Date).First().Date;
        return (from item in cashFlow
            let daysRatio = -(item.Date - startDate).Days / DaysPerYear
            select item.Amount * daysRatio * Math.Pow(1.0 + rate, daysRatio - 1)).Sum();
    }

    private readonly struct Brackets
    {
        public readonly double First;
        public readonly double Second;

        private Brackets(double first, double second)
        {
            First = first;
            Second = second;
        }

        internal static Brackets Find(Func<IList<CashItem>, double, double> f, IList<CashItem> cashFlow, double guess = DefaultGuess,
            int maxIterations = MaxIterations)
        {
            const double bracketStep = 0.5;
            var leftBracket = guess - bracketStep;
            var rightBracket = guess + bracketStep;
            var i = 0;
            while (f(cashFlow, leftBracket) * f(cashFlow, rightBracket) > 0 && i++ < maxIterations)
            {
                leftBracket -= bracketStep;
                rightBracket += bracketStep;
            }

            return i >= maxIterations ? new Brackets(0, 0) : new Brackets(leftBracket, rightBracket);
        }
    }

    public readonly struct CashItem(DateTime date, double amount) : IComparable<CashItem>
    {
        public readonly DateTime Date = date;
        public readonly double Amount = amount;

        public int CompareTo(CashItem other)
        {
            return Date.CompareTo(other.Date);
        }

        public override string ToString()
        {
            return $"{Date:G} {Amount:N2}";
        }
    }

    public class XIRRException : Exception
    {
        public XIRRException(string message) : base(message)
        {
        }

        public XIRRException()
        {
        }

        public XIRRException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public class XIRRArgumentException : Exception
    {
        public XIRRArgumentException(string message) : base(message)
        {
        }

        public XIRRArgumentException()
        {
        }

        public XIRRArgumentException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}