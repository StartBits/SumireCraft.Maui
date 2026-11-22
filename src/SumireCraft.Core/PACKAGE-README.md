# SumireCraft.Core

What the SumireCraft packages share that depends on no UI framework.

- **DateTimeInput** -- a date and a time that are entered separately and can each be empty.

.NET 10 · trimming and Native AOT compatible · MIT

## Getting started

```csharp
using SumireCraft.DateTimes;

var start = new DateTimeInput { DateAsDateOnly = new DateOnly(2026, 11, 8) };

bool complete = start.IsComplete;   // false: no time yet
DateTime? saved = start;            // 2026-11-08 00:00:00
```

## Documentation

- [README on GitHub](https://github.com/startbits/SumireCraft.Maui#readme)
- [Changelog](https://github.com/startbits/SumireCraft.Maui/blob/main/CHANGELOG.md)
- [Issues](https://github.com/startbits/SumireCraft.Maui/issues)
