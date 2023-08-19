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

|                      Method |      Mean |     Error |    StdDev |   Gen0 |   Gen1 | Allocated |
|---------------------------- |----------:|----------:|----------:|-------:|-------:|----------:|
|               StringReplace |  58.26 ns |  0.651 ns |  0.577 ns | 0.0143 |      - |     240 B |
|       StringBuilderAppend64 | 104.48 ns |  1.092 ns |  0.968 ns | 0.0119 |      - |     200 B |
|         StringPlusOperation | 116.64 ns |  1.024 ns |  0.855 ns | 0.0181 |      - |     304 B |
|           MutStringAppend64 | 118.98 ns |  1.703 ns |  1.593 ns | 0.0143 |      - |     240 B |
|         StringInterpolation | 119.53 ns |  1.366 ns |  1.211 ns | 0.0067 |      - |     112 B |
|             MutStringAppend | 129.21 ns |  1.162 ns |  1.087 ns | 0.0143 |      - |     240 B |
|                StringConcat | 142.30 ns |  2.013 ns |  1.883 ns | 0.0315 |      - |     528 B |
|                  StringJoin | 156.33 ns |  1.426 ns |  1.334 ns | 0.0272 |      - |     456 B |
|            MutStringReplace | 161.98 ns |  1.953 ns |  1.731 ns | 0.0110 |      - |     184 B |
|         StringBuilderAppend | 206.72 ns |  0.981 ns |  0.869 ns | 0.0257 |      - |     432 B |
|    LargeStringPlusOperation | 208.63 ns |  4.012 ns |  4.120 ns | 0.4795 |      - |    8024 B |
|           LargeStringConcat | 210.14 ns |  4.190 ns |  4.988 ns | 0.4795 |      - |    8024 B |
|    LargeStringBuilderAppend | 283.05 ns |  5.703 ns |  6.789 ns | 0.4959 | 0.0153 |    8296 B |
|          PrimitiveMutString | 357.72 ns |  2.085 ns |  1.951 ns | 0.0296 |      - |     496 B |
|       PrimitiveStringConcat | 384.43 ns |  5.285 ns |  4.944 ns | 0.0648 |      - |    1088 B |
|        LargeMutStringAppend | 387.31 ns |  7.570 ns |  8.414 ns | 0.4935 |      - |    8248 B |
| PrimitiveStringInterpolated | 396.32 ns |  2.445 ns |  2.287 ns | 0.0172 |      - |     288 B |
|      PrimitiveStringBuilder | 408.63 ns |  2.105 ns |  1.866 ns | 0.0334 |      - |     560 B |
|PrimitiveStringPlusOperation | 439.59 ns |  3.644 ns |  3.408 ns | 0.0515 |      - |     864 B |
|     LargeStringInterpolated | 456.81 ns |  8.659 ns |  9.265 ns | 0.4792 |      - |    8024 B |
|        StringBuilderReplace | 761.98 ns | 15.126 ns | 18.007 ns | 0.0219 |      - |     368 B |

## Supported Platforms

* .NET Core 6.0+
* .NET Framework 4.6+
* .NET Standard 1.3+

## How do I use it?
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
    
```

### Using `MutString`

```C#
    // Retrieve an instance from the pool.
    var ms = MutString.Create();
    
    ms.Append("Cost: ")
      .Append(32.11)
      .Append(" Sent: ")
      .Append(false);
	
    ms[0] = 'A';
    char chr = ms[0];
	
    ms.Span.Fill('B');
    
    // Return instance of string.
    ms.ToString();
```
