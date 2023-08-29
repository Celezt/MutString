# MutString
An alternative to the `System.Text.StringBuilder` C# class. Modified version of [LiteStringBuilder](https://github.com/justinamiller/LiteStringBuilder).

## Why?
Because `System.Text.StringBuilder` does a lot of unnecessary memory allocation when appending data and is overall slow with its implementations. It is also not always continuous data but instead spread across in chunks, disallowing the use of `Span`. `MutString` is designed for smaller strings that are frequently modified instead of document-sized text.

## Possible Improvements
`MutString` is faster than [LiteStringBuilder](https://github.com/justinamiller/LiteStringBuilder) with major changes to the existing methods. Some of the implementations are based on `String`'s source code. Even with those changes, there are still possibilities for improvement. 

#### Single, Double, Decimal
Floats currently relies on `.ToString` because it uses [Dragon4](https://www.ryanjuckett.com/printing-floating-point-numbers-part-2-dragon4/), which is hard to compete with. .NET does not in the latest version expose any of the methods used to implement it, and has too many dependencies realistically to copy.

#### Params Append
It currently loops and individually appends each element. This could be improved by allocating enough space before appending. 

#### Trim Character
`Trim`, `TrimStart` and `TrimEnd` is supported for removing empty space. `String` supports trimming by specified characters which `MutString` does not support.

#### Append Join
`System.Text.StringBuilder` supports appending multiple elements with separators which `MutString` lacks. 

#### String Comparison
It currently does not support `StringComparison` for many of its methods which would benefit from it. Implementing `StringComparison` in a performative way is not easy and would most likely need different algorithms depending on the comparison type.

## Performance
[Benchmark Test](https://github.com/Celezt/MutString/blob/main/perf/Benchmark.NET/MutStringBenchmark.cs)

##### .NET 7
``` ini
BenchmarkDotNet v0.13.7, Windows 11 (10.0.22621.2134/22H2/2022Update/SunValley2)
AMD Ryzen 7 7700, 1 CPU, 16 logical and 8 physical cores
.NET SDK 7.0.400
  [Host]     : .NET 7.0.10 (7.0.1023.36312), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.10 (7.0.1023.36312), X64 RyuJIT AVX2
```

|                       Method |      Mean |     Error |    StdDev |   Gen0 |   Gen1 | Allocated |
|----------------------------- |----------:|----------:|----------:|-------:|-------:|----------:|
|                StringReplace |  58.74 ns |  0.269 ns |  0.239 ns | 0.0143 |      - |     240 B |
|          StringPlusOperation | 114.86 ns |  0.401 ns |  0.335 ns | 0.0181 |      - |     304 B |
|          StringInterpolation | 117.50 ns |  0.981 ns |  0.819 ns | 0.0067 |      - |     112 B |
|              MutStringAppend | 131.50 ns |  0.499 ns |  0.466 ns | 0.0143 |      - |     240 B |
|                 StringConcat | 141.66 ns |  1.172 ns |  1.096 ns | 0.0315 |      - |     528 B |
|                   StringJoin | 156.69 ns |  1.015 ns |  0.950 ns | 0.0272 |      - |     456 B |
|             MutStringReplace | 162.85 ns |  0.284 ns |  0.237 ns | 0.0110 |      - |     184 B |
|          StringBuilderAppend | 200.80 ns |  1.231 ns |  1.152 ns | 0.0257 |      - |     432 B |
|     LargeStringPlusOperation | 205.03 ns |  4.108 ns |  5.484 ns | 0.4795 |      - |    8024 B |
|            LargeStringConcat | 214.53 ns |  4.170 ns |  4.283 ns | 0.4795 |      - |    8024 B |
|     LargeStringBuilderAppend | 275.13 ns |  4.988 ns |  5.122 ns | 0.4959 | 0.0153 |    8296 B |
|     PrimitiveMutStringAppend | 353.22 ns |  2.398 ns |  2.243 ns | 0.0296 |      - |     496 B |
|         LargeMutStringAppend | 368.39 ns |  7.036 ns |  6.582 ns | 0.4935 |      - |    8248 B |
|        PrimitiveStringConcat | 383.77 ns |  2.725 ns |  2.549 ns | 0.0648 |      - |    1088 B |
|  PrimitiveStringInterpolated | 402.43 ns |  1.352 ns |  1.199 ns | 0.0172 |      - |     288 B |
| PrimitiveStringBuilderAppend | 410.15 ns |  2.230 ns |  2.086 ns | 0.0334 |      - |     560 B |
| PrimitiveStringPlusOperation | 427.88 ns |  1.576 ns |  1.316 ns | 0.0515 |      - |     864 B |
|      LargeStringInterpolated | 440.49 ns |  8.043 ns |  7.524 ns | 0.4792 |      - |    8024 B |
|         StringBuilderReplace | 754.67 ns | 15.028 ns | 17.306 ns | 0.0219 |      - |     368 B |

## Supported Platforms

* .NET Core 6.0+
* .NET Framework 4.6+
* .NET Standard 1.3+

## How to Use it

Install the `Nuget` package:

```powershell
    dotnet add package Celezt.Text.MutString --version 2.2.0
```
[Package](https://www.nuget.org/packages/Celezt.Text.MutString/)

Or reference the MutString.dll assembly that matches your app's platform.

### Creating `MutString`

```C#
    using Celezt.String;
    
    // Create through instance.
    var ms = new MutString();
    
    // Or through static call (will create a new instance).
    var ms = MutString.Create();
    
    // Can create an instance with buffer pool size.
    var ms = new MutString(500);
    
    // Can create an instance with an initial string value.
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
