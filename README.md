# MutString
An alternative to the `System.Text.StringBuilder` C# class. Modified version of [LiteStringBuilder](https://github.com/justinamiller/LiteStringBuilder).

## Why?
Because `System.Text.StringBuilder` actually does a lot of memory allocation when appending strings, often it's just not better than a direct string `concat`. It is also not always continuous data, but instead spread out in chunks, disallowing the use of `Span`.

### Performance
[Benchmark Test](https://github.com/Celezt/MutString/blob/main/perf/Benchmark.NET/MutStringBenchmark.cs)

##### .NET 7
``` ini
BenchmarkDotNet v0.13.7, Windows 11 (10.0.22621.2134/22H2/2022Update/SunValley2)
AMD Ryzen 7 7700, 1 CPU, 16 logical and 8 physical cores
.NET SDK 7.0.400
  [Host]     : .NET 7.0.10 (7.0.1023.36312), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.10 (7.0.1023.36312), X64 RyuJIT AVX2
```

|                 Method |     Mean |   Error |  StdDev | Rank |   Gen0 | Allocated |
|----------------------- |---------:|--------:|--------:|-----:|-------:|----------:|
| StringBuilderAppend128 | 105.3 ns | 0.96 ns | 0.89 ns |    1 | 0.0257 |     432 B |
|    StringInterpolation | 117.3 ns | 1.04 ns | 0.97 ns |    2 | 0.0067 |     112 B |
|    StringPlusOperation | 119.7 ns | 1.89 ns | 1.77 ns |    3 | 0.0181 |     304 B |
|     MutStringAppend128 | 121.7 ns | 1.16 ns | 1.03 ns |    3 | 0.0262 |     440 B |
|        MutStringAppend | 134.5 ns | 1.05 ns | 0.98 ns |    4 | 0.0186 |     312 B |
|           StringConcat | 140.4 ns | 0.72 ns | 0.60 ns |    5 | 0.0315 |     528 B |
|             StringJoin | 153.6 ns | 1.53 ns | 1.35 ns |    6 | 0.0272 |     456 B |
|    StringBuilderAppend | 212.5 ns | 1.67 ns | 1.39 ns |    7 | 0.0319 |     536 B |

## Supported Platforms

* .NET Core 7.0+
* .NET Framework 4.6+
* .NET Standard 1.3+

## How do I use it?
`MutString` is a struct and needs to be `Clone` to allocate a new buffer. It is modifiable directly from `Span`.
### Creating `MutString`

```C#
    using Celezt.String;
    
    // Create through instance.
    var ms = new MutString();
    
    // Or through static call (will create a new instance).
    var ms = MutString.Create();
    
    // Can create instance with buffer pool size.
    var ms = new MutString(500);
    
    // Can create instance with initial string value.
    var ms = new MutString("Hello World");
    
    // Can be cloned to instance with a new buffer.
    var ms2 = ms.Clone();
```

### Using `MutString`

```C#
    // Retrieve an instance from the pool.
    var ms = MutString.Create();
    
    ms.Append("Cost: ");
    ms.Append(32.11)
    ms.Append(" Sent: ")
    ms.Append(false);

    ms[0] = 'A';
    char chr = ms[0];

    ms.Span.Fill('B');
    
    // Return instance of string.
    ms.ToString();
``
