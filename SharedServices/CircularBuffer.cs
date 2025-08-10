namespace BruSoftware.SharedServices;

/// <summary>
/// A simple circular buffer implementation that also allows fast direct access to the underlying array elements.
/// No argument checking etc. is done. You must add Size values to the buffer before non-zero values will be returned from some members.
/// Currently this is not thread-safe.
/// </summary>
/// <typeparam name="T"></typeparam>
public class CircularBuffer<T>
{
    private readonly T[] _array;
    private long _head; // index of the head (first element) of the buffer

    public CircularBuffer(long size)
    {
        Size = size;
        _array = new T[Size];
    }

    public long Size { get; }

    /// <summary>
    /// Return the element at index. No checking is done to see if it is a valid element.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public T this[long index] => _array[index];

    public void Add(T item)
    {
        _array[_head++] = item;
        if (_head >= Size)
        {
            _head = 0;
        }
    }

    /// <summary>
    /// Return the first element in the buffer. This only is valid after Size elements have been added.
    /// </summary>
    /// <returns></returns>
    public T Peek()
    {
        return _array[_head];
    }

    /// <summary>
    /// Return the last element in the buffer. This only is valid after Size elements have been added.
    /// </summary>
    /// <returns></returns>
    public T PeekTail()
    {
        var tail = _head - 1;
        if (tail < 0)
        {
            tail = Size - 1;
        }
        return _array[tail];
    }
}