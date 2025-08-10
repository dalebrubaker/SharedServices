# BruSoftware.SharedServices

A comprehensive collection of shared services and utilities for .NET applications.

## Features

- **Async Helpers**: Utilities for asynchronous programming
- **Configuration Management**: JSON-based configuration with custom resolvers
- **Extension Methods**: Extensions for common types (DateTime, String, Collections, etc.)
- **Attributes**: Custom attributes for PropertyGrid and serialization
- **Data Structures**: CircularBuffer, BitArrayFast, TreeNode
- **Utilities**: Math utilities, XIRR calculations, Fibonacci, Permutations
- **Service Locator**: Dependency injection helper
- **Converters**: Type converters and JSON converters

## Installation

```bash
dotnet add package BruSoftware.SharedServices
```

## Usage

```csharp
using BruSoftware.SharedServices;
using BruSoftware.SharedServices.ExtensionMethods;

// Use extension methods
var rounded = myDouble.RoundToSignificantDigits(3);
var isWeekend = DateTime.Now.IsWeekend();

// Use utilities
var fibonacci = Fibonacci.GetNumber(10);
var xirr = XIRR.Calculate(cashFlows, dates);
```

## Requirements

- .NET 9.0 or higher
- x64 platform

## Dependencies

- Microsoft.CSharp
- Microsoft.Extensions.Configuration
- Newtonsoft.Json
- NLog
- BruSoftware.ListMmf

## License

MIT License

## Contributing

Contributions are welcome! Please submit pull requests or open issues on GitHub.