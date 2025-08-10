// My version of the Microsoft BitArray Class
// Added GetCardinality
// Removed IEnumerable in order to remove version checking (that collection was modified during enumeration) -- faster Set() by 20% or so

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

//using System.Diagnostics.Contracts;

namespace BruSoftware.SharedServices;

// A vector of bits.  Use this to store bits efficiently, without having to do bit
// shifting yourself.
// This is a buffered read-only list. No checking is made that writes were not made during the enumeration.
[ComVisible(true)]
[Serializable]
public sealed class BitArrayFast : ICollection, ICloneable, IReadOnlyList<bool>
{
    // XPerY=n means that n Xs can be stored in 1 Y.
    private const int BitsPerInt32 = 32;

    private const int BytesPerInt32 = 4;
    private const int BitsPerByte = 8;

    private const int ShrinkThreshold = 256;

    [NonSerialized] private object _syncRoot;

    private BitArrayFast()
    {
    }

    /*=========================================================================
    ** Allocates space to hold length bit values. All of the values in the bit
    ** array are set to false.
    **
    ** Exceptions: ArgumentException if length < 0.
    =========================================================================*/

    public BitArrayFast(int length)
        : this(length, false)
    {
    }

    /*=========================================================================
    ** Allocates space to hold length bit values. All of the values in the bit
    ** array are set to defaultValue.
    **
    ** Exceptions: ArgumentOutOfRangeException if length < 0.
    =========================================================================*/

    public BitArrayFast(int length, bool defaultValue)
    {
        if (length < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length), MyBitArrayEnvironment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
        }

        //Contract.EndContractBlock();

        IntArray = new int[GetArrayLength(length, BitsPerInt32)];
        Count = length;

        var fillValue = defaultValue ? unchecked((int)0xffffffff) : 0;
        for (var i = 0; i < IntArray.Length; i++)
        {
            IntArray[i] = fillValue;
        }
    }

    /*=========================================================================
    ** Allocates space to hold the bit values in bytes. bytes[0] represents
    ** bits 0 - 7, bytes[1] represents bits 8 - 15, etc. The LSB of each byte
    ** represents the lowest index value; bytes[0] & 1 represents bit 0,
    ** bytes[0] & 2 represents bit 1, bytes[0] & 4 represents bit 2, etc.
    **
    ** Exceptions: ArgumentException if bytes == null.
    =========================================================================*/

    public BitArrayFast(byte[] bytes)
    {
        if (bytes == null)
        {
            throw new ArgumentNullException(nameof(bytes));
        }

        //Contract.EndContractBlock();
        // this value is chosen to prevent overflow when computing m_length.
        // m_length is of type int32 and is exposed as a property, so
        // type of m_length can't be changed to accommodate.
        if (bytes.Length > int.MaxValue / BitsPerByte)
        {
            throw new ArgumentException(MyBitArrayEnvironment.GetResourceString("Argument_ArrayTooLarge", BitsPerByte), nameof(bytes));
        }

        IntArray = new int[GetArrayLength(bytes.Length, BytesPerInt32)];
        Count = bytes.Length * BitsPerByte;

        var i = 0;
        var j = 0;
        while (bytes.Length - j >= 4)
        {
            IntArray[i++] = (bytes[j] & 0xff)
                            | ((bytes[j + 1] & 0xff) << 8)
                            | ((bytes[j + 2] & 0xff) << 16)
                            | ((bytes[j + 3] & 0xff) << 24);
            j += 4;
        }

        //Contract.Assert(bytes.Length - j >= 0, "BitArrayFast byteLength problem");
        //Contract.Assert(bytes.Length - j < 4, "BitArrayFast byteLength problem #2");

        switch (bytes.Length - j)
        {
            case 3:
                IntArray[i] = (bytes[j + 2] & 0xff) << 16;
                goto case 2;

            // fall through
            case 2:
                IntArray[i] |= (bytes[j + 1] & 0xff) << 8;
                goto case 1;

            // fall through
            case 1:
                IntArray[i] |= bytes[j] & 0xff;
                break;
        }
    }

    public BitArrayFast(bool[] values)
    {
        if (values == null)
        {
            throw new ArgumentNullException(nameof(values));
        }

        //Contract.EndContractBlock();

        IntArray = new int[GetArrayLength(values.Length, BitsPerInt32)];
        Count = values.Length;

        for (var i = 0; i < values.Length; i++)
        {
            if (values[i])
            {
                IntArray[i / 32] |= 1 << (i % 32);
            }
        }
    }

    /*=========================================================================
    ** Allocates space to hold the bit values in values. values[0] represents
    ** bits 0 - 31, values[1] represents bits 32 - 63, etc. The LSB of each
    ** integer represents the lowest index value; values[0] & 1 represents bit
    ** 0, values[0] & 2 represents bit 1, values[0] & 4 represents bit 2, etc.
    **
    ** Exceptions: ArgumentException if values == null.
    =========================================================================*/

    public BitArrayFast(int[] values)
    {
        if (values == null)
        {
            throw new ArgumentNullException(nameof(values));
        }

        //Contract.EndContractBlock();
        // this value is chosen to prevent overflow when computing m_length
        if (values.Length > int.MaxValue / BitsPerInt32)
        {
            throw new ArgumentException(MyBitArrayEnvironment.GetResourceString("Argument_ArrayTooLarge", BitsPerInt32), nameof(values));
        }

        IntArray = new int[values.Length];
        Count = values.Length * BitsPerInt32;

        Array.Copy(values, IntArray, values.Length);
    }

    /*=========================================================================
    ** Allocates a new BitArrayFast with the same length and bit values as bits.
    **
    ** Exceptions: ArgumentException if bits == null.
    =========================================================================*/

    public BitArrayFast(BitArrayFast bits)
    {
        if (bits == null)
        {
            throw new ArgumentNullException(nameof(bits));
        }

        //Contract.EndContractBlock();

        var arrayLength = GetArrayLength(bits.Count, BitsPerInt32);
        IntArray = new int[arrayLength];
        Count = bits.Count;

        Array.Copy(bits.IntArray, IntArray, arrayLength);
    }

    public int Length
    {
        //Contract.Ensures(Contract.Result<int>() >= 0);
        get => Count;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), MyBitArrayEnvironment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }

            //Contract.EndContractBlock();

            var newints = GetArrayLength(value, BitsPerInt32);
            if (newints > IntArray.Length || newints + ShrinkThreshold < IntArray.Length)
            {
                // grow or shrink (if wasting more than _ShrinkThreshold ints)
                var newarray = new int[newints];
                Array.Copy(IntArray, newarray, newints > IntArray.Length ? IntArray.Length : newints);
                IntArray = newarray;
            }

            if (value > Count)
            {
                // clear high bit values in the last int
                var last = GetArrayLength(Count, BitsPerInt32) - 1;
                var bits = Count % 32;
                if (bits > 0)
                {
                    IntArray[last] &= (1 << bits) - 1;
                }

                // clear remaining int values
                Array.Clear(IntArray, last + 1, newints - last - 1);
            }

            Count = value;
        }
    }

    public bool IsReadOnly => false;

    /// <summary>
    /// Get the underlying int32s
    /// </summary>
    public int[] IntArray { get; private set; }

    public object Clone()
    {
        //Contract.Ensures(Contract.Result<Object>() != null);
        //Contract.Ensures(((BitArrayFast)Contract.Result<Object>()).Length == this.Length);

        var bitArray = new BitArrayFast(IntArray) { Count = Count };
        return bitArray;
    }

    // ICollection implementation
    public void CopyTo(Array array, int index)
    {
        if (array == null)
        {
            throw new ArgumentNullException(nameof(array));
        }

        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index), MyBitArrayEnvironment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
        }

        if (array.Rank != 1)
        {
            throw new ArgumentException(MyBitArrayEnvironment.GetResourceString("Arg_RankMultiDimNotSupported"));
        }

        //Contract.EndContractBlock();

        if (array is int[])
        {
            Array.Copy(IntArray, 0, array, index, GetArrayLength(Count, BitsPerInt32));
        }
        else if (array is byte[])
        {
            var arrayLength = GetArrayLength(Count, BitsPerByte);
            if (array.Length - index < arrayLength)
            {
                throw new ArgumentException(MyBitArrayEnvironment.GetResourceString("Argument_InvalidOffLen"));
            }

            var b = (byte[])array;
            for (var i = 0; i < arrayLength; i++)
            {
                b[index + i] = (byte)((IntArray[i / 4] >> (i % 4 * 8)) & 0x000000FF); // Shift to bring the required byte to LSB, then mask
            }
        }
        else if (array is bool[])
        {
            if (array.Length - index < Count)
            {
                throw new ArgumentException(MyBitArrayEnvironment.GetResourceString("Argument_InvalidOffLen"));
            }

            var b = (bool[])array;
            for (var i = 0; i < Count; i++)
            {
                b[index + i] = ((IntArray[i / 32] >> (i % 32)) & 0x00000001) != 0;
            }
        }
        else
        {
            throw new ArgumentException(MyBitArrayEnvironment.GetResourceString("Arg_BitArrayTypeUnsupported"));
        }
    }

    public int Count { get; private set; }

    public object SyncRoot
    {
        get
        {
            if (_syncRoot == null)
            {
                Interlocked.CompareExchange<object>(ref _syncRoot, new object(), null);
            }
            return _syncRoot;
        }
    }

    public bool IsSynchronized => false;

    public IEnumerator GetEnumerator()
    {
        throw new NotImplementedException();
    }

    public bool this[int index]
    {
        get => Get(index);
        set => Set(index, value);
    }

    IEnumerator<bool> IEnumerable<bool>.GetEnumerator()
    {
        throw new NotImplementedException();
    }

    /*=========================================================================
    ** Returns the bit value at position index.
    **
    ** Exceptions: ArgumentOutOfRangeException if index < 0 or
    **             index >= GetLength().
    =========================================================================*/

    public bool Get(int index)
    {
        if (index < 0 || index >= Length)
        {
            throw new ArgumentOutOfRangeException(nameof(index), MyBitArrayEnvironment.GetResourceString("ArgumentOutOfRange_Index"));
        }

        //Contract.EndContractBlock();

        return (IntArray[index / 32] & (1 << (index % 32))) != 0;
    }

    /*=========================================================================
     ** Returns the bit value at position index.
     **
     ** Exceptions: ArgumentOutOfRangeException if index < 0 or
     **             index >= GetLength().
     =========================================================================*/

    public bool GetNoArgCheck(int index)
    {
        //if (index < 0 || index >= Length) {
        //    throw new ArgumentOutOfRangeException("index", MyBitArrayEnvironment.GetResourceString("ArgumentOutOfRange_Index"));
        //}
        //Contract.EndContractBlock();

        return (IntArray[index / 32] & (1 << (index % 32))) != 0;
    }

    /*=========================================================================
    ** Sets the bit value at position index to value.
    **
    ** Exceptions: ArgumentOutOfRangeException if index < 0 or
    **             index >= GetLength().
    =========================================================================*/

    public void Set(int index, bool value)
    {
        if (index < 0 || index >= Length)
        {
            throw new ArgumentOutOfRangeException(nameof(index), MyBitArrayEnvironment.GetResourceString("ArgumentOutOfRange_Index"));
        }

        //Contract.EndContractBlock();

        if (value)
        {
            IntArray[index / 32] |= 1 << (index % 32);
        }
        else
        {
            IntArray[index / 32] &= ~(1 << (index % 32));
        }
    }

    /*=========================================================================
    ** Sets the bit values from lowerBound up to but not including upperBound to value.
    **
    ** Exceptions: ArgumentOutOfRangeException if index < 0 or
    **             index >= GetLength().
    =========================================================================*/

    public void Set(int lowerBound, int upperBound, bool value)
    {
        MyDebug.Assert(upperBound > lowerBound);
        if (lowerBound < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(lowerBound), MyBitArrayEnvironment.GetResourceString("ArgumentOutOfRange_Index"));
        }
        if (upperBound > Length)
        {
            throw new ArgumentOutOfRangeException(nameof(upperBound), MyBitArrayEnvironment.GetResourceString("ArgumentOutOfRange_Index"));
        }

        //Contract.EndContractBlock();
        if (value)
        {
            for (var index = lowerBound; index < upperBound; index++)
            {
                IntArray[index / 32] |= 1 << (index % 32);
            }
        }
        else
        {
            for (var index = lowerBound; index < upperBound; index++)
            {
                IntArray[index / 32] &= ~(1 << (index % 32));
            }
        }
    }

    /*=========================================================================
    ** Sets the bit value at position index to value.
    **
    ** Exceptions: ArgumentOutOfRangeException if index < 0 or
    **             index >= GetLength().
    =========================================================================*/

    public void SetNoArgCheck(int index, bool value)
    {
        //if (index < 0 || index >= Length) {
        //    throw new ArgumentOutOfRangeException("index", MyBitArrayEnvironment.GetResourceString("ArgumentOutOfRange_Index"));
        //}
        //Contract.EndContractBlock();

        if (value)
        {
            IntArray[index / 32] |= 1 << (index % 32);
        }
        else
        {
            IntArray[index / 32] &= ~(1 << (index % 32));
        }
    }

    /*=========================================================================
    ** Sets all the bit values to value.
    =========================================================================*/

    public void SetAll(bool value)
    {
        var fillValue = value ? unchecked((int)0xffffffff) : 0;
        var ints = GetArrayLength(Count, BitsPerInt32);
        for (var i = 0; i < ints; i++)
        {
            IntArray[i] = fillValue;
        }
    }

    /*=========================================================================
    ** Returns a reference to the current instance ANDed with value.
    **
    ** Exceptions: ArgumentException if value == null or
    **             value.Length != this.Length.
    =========================================================================*/

    public BitArrayFast And(BitArrayFast value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }
        if (Length != value.Length)
        {
            throw new ArgumentException(MyBitArrayEnvironment.GetResourceString("Arg_ArrayLengthsDiffer"));
        }

        //Contract.EndContractBlock();

        var ints = GetArrayLength(Count, BitsPerInt32);
        for (var i = 0; i < ints; i++)
        {
            IntArray[i] &= value.IntArray[i];
        }
        return this;
    }

    /*=========================================================================
    ** Returns a reference to the current instance ORed with value.
    **
    ** Exceptions: ArgumentException if value == null or
    **             value.Length != this.Length.
    =========================================================================*/

    public BitArrayFast Or(BitArrayFast value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }
        if (Length != value.Length)
        {
            throw new ArgumentException(MyBitArrayEnvironment.GetResourceString("Arg_ArrayLengthsDiffer"));
        }

        //Contract.EndContractBlock();

        var ints = GetArrayLength(Count, BitsPerInt32);
        for (var i = 0; i < ints; i++)
        {
            IntArray[i] |= value.IntArray[i];
        }
        return this;
    }

    /*=========================================================================
    ** Returns a reference to the current instance XORed with value.
    **
    ** Exceptions: ArgumentException if value == null or
    **             value.Length != this.Length.
    =========================================================================*/

    public BitArrayFast Xor(BitArrayFast value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }
        if (Length != value.Length)
        {
            throw new ArgumentException(MyBitArrayEnvironment.GetResourceString("Arg_ArrayLengthsDiffer"));
        }

        //Contract.EndContractBlock();

        var ints = GetArrayLength(Count, BitsPerInt32);
        for (var i = 0; i < ints; i++)
        {
            IntArray[i] ^= value.IntArray[i];
        }
        return this;
    }

    /*=========================================================================
    ** Inverts all the bit values. On/true bit values are converted to
    ** off/false. Off/false bit values are turned on/true. The current instance
    ** is updated and returned.
    =========================================================================*/

    public BitArrayFast Not()
    {
        var ints = GetArrayLength(Count, BitsPerInt32);
        for (var i = 0; i < ints; i++)
        {
            IntArray[i] = ~IntArray[i];
        }
        return this;
    }

    /// <summary>
    /// Return the number of set bits in this bitArray
    /// https://stackoverflow.com/questions/5063178/counting-bits-set-in-a-net-bitarray-class
    /// </summary>
    /// <returns></returns>
    public int GetCardinality()
    {
        var count = 0;
        var arrayCount = GetArrayLength(Length, BitsPerInt32); // ignore extra array values
        for (var i = 0; i < arrayCount; i++)
        {
            var c = IntArray[i];

            // magic (http://graphics.stanford.edu/~seander/bithacks.html#CountBitsSetParallel)
            unchecked
            {
#pragma warning disable RCS1058 // Use compound assignment.
                c = c - ((c >> 1) & 0x55555555);
                c = (c & 0x33333333) + ((c >> 2) & 0x33333333);
                c = (((c + (c >> 4)) & 0xF0F0F0F) * 0x1010101) >> 24;
#pragma warning restore RCS1058 // Use compound assignment.
            }
            count += c;
        }
        return count;
    }

    /// <summary>
    ///     <para>
    /// Used for conversion between different representations of bit array.
    /// Returns (n+(div-1))/div, rearranged to avoid arithmetic overflow.
    /// For example, in the bit to int case, the straightforward calc would
    /// be (n+31)/32, but that would cause overflow. So instead it's
    /// rearranged to ((n-1)/32) + 1, with special casing for 0.
    ///     </para>
    ///     <para>
    /// Usage:
    /// GetArrayLength(77, BitsPerInt32): returns how many ints must be
    /// allocated to store 77 bits.
    ///     </para>
    /// </summary>
    /// <param name="n"></param>
    /// <param name="div">
    /// use a conversion constant, e.g. BytesPerInt32 to get
    /// how many ints are required to store n bytes
    /// </param>
    /// <returns></returns>
    private static int GetArrayLength(int n, int div)
    {
        //Contract.Assert(div > 0, "GetArrayLength: div arg must be greater than 0");
        return n > 0 ? (n - 1) / div + 1 : 0;
    }

    /// <summary>
    /// This is a buffered read-only list. No checking is made that writes were not made during the enumeration.
    /// </summary>
    [Serializable]
    public struct Enumerator : IEnumerator<bool>, IEnumerator
    {
        private readonly BitArrayFast _list;
        private int _index;

        internal Enumerator(BitArrayFast list)
        {
            _list = list;
            _index = 0;
            Current = false;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// This is a buffered read-only list. No checking is made that writes were not made during the enumeration.
        /// </summary>
        /// <returns></returns>
        public bool MoveNext()
        {
            var localList = _list;
            if ((uint)_index < (uint)localList.Count)
            {
                Current = localList[_index];
                _index++;
                return true;
            }
            return MoveNextRare();
        }

        private bool MoveNextRare()
        {
            _index = _list.Count + 1;
            Current = false;
            return false;
        }

        public bool Current { get; private set; }

        object IEnumerator.Current
        {
            get
            {
                if (_index == 0 || _index == _list.Count + 1)
                {
                    throw new InvalidOperationException("Enum Op Cant Happen");
                }
                return Current;
            }
        }

        void IEnumerator.Reset()
        {
            _index = 0;
            Current = false;
        }
    }
}

public static class MyBitArrayEnvironment
{
    public static string GetResourceString(string str)
    {
        return str;
    }

    public static string GetResourceString(string str, int i)
    {
        return $"{str} {i}";
    }
}