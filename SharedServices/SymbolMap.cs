using System.Collections.Generic;
using System.Threading;

namespace BruSoftware.SharedServices;

/// <summary>
/// Bidirectional map of internal symbol to DTC symbol.
/// Thanks to Enigmativity at https://stackoverflow.com/questions/10966331/two-way-bidirectional-dictionary-in-c/10966684#10966684
/// </summary>
public class SymbolMap
{
    private readonly object _lock = new();
    private readonly string _name;
    private readonly Dictionary<string, string> _symbolBySymbolAdapter = new();
    private readonly Dictionary<string, string> _symbolAdapterBySymbol = new();

    public SymbolMap(string name)
    {
        _name = name;
        // _symbolAdapterBySymbol = new Indexer<string, string>(_symbolDTCBySymbol);
        // _symbolBySymbolAdapter = new Indexer<string, string>(_symbolBySymbolDTC);
    }

    // public Indexer<string, string> _symbolBySymbolAdapter { get; }
    // public Indexer<string, string> _symbolBySymbolAdapter { get; }
    public int Count => _symbolAdapterBySymbol.Count;

    /// <summary>
    /// Add the symbol pair
    /// </summary>
    /// <param name="symbol">some symbol to pair with symbolAdapter, e.g. internal to the client or server application</param>
    /// <param name="symbolAdapter">the symbol to be sent/received via DTC</param>
    public void Add(string symbol, string symbolAdapter)
    {
        lock (_lock)
        {
            _symbolAdapterBySymbol.Add(symbol, symbolAdapter);
            _symbolBySymbolAdapter.Add(symbolAdapter, symbol);
        }
    }

    /// <summary>
    /// Try to get the value of symbolAdapter for the given symbol
    /// </summary>
    /// <param name="symbol"></param>
    /// <param name="symbolAdapter"></param>
    /// <returns></returns>
    public bool TryGetValueSymbolAdapter(string symbol, out string symbolAdapter)
    {
        lock (_lock)
        {
            var result = _symbolAdapterBySymbol.TryGetValue(symbol, out symbolAdapter);
            return result;
        }
    }

    /// <summary>
    /// Try to get the value of symbol for the given symbolAdapter
    /// </summary>
    /// <param name="symbolAdapter"></param>
    /// <param name="symbol"></param>
    /// <returns></returns>
    public bool TryGetValueSymbol(string symbolAdapter, out string symbol)
    {
        lock (_lock)
        {
            var result = _symbolBySymbolAdapter.TryGetValue(symbolAdapter, out symbol);
            return result;
        }
    }

    public void Clear()
    {
        _symbolBySymbolAdapter.Clear();
        _symbolAdapterBySymbol.Clear();
    }

    public override string ToString()
    {
        return $"{_name} Count={Count:N0}";
    }

    public class Indexer<T3, T4>
    {
        private readonly Dictionary<T3, T4> _dictionary;

        public Indexer(Dictionary<T3, T4> dictionary)
        {
            _dictionary = dictionary;
        }

        public T4 this[T3 index]
        {
            get => _dictionary[index];
            set => _dictionary[index] = value;
        }
    }
}