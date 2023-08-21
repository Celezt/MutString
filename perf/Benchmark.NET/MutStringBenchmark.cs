#define StandardTest
#define LargeTest
#define PrimitiveTest

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using System.Text;
using Celezt.Text;

[MemoryDiagnoser, Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class MutStringBenchmark
{
#if StandardTest
    #region Standard
    [Benchmark]
    public string StringInterpolation()
    {
        return $"PI= {Math.PI} _373= {373} {true} {short.MaxValue}{'z'}";
    }

    [Benchmark]
    public string StringPlusOperation()
    {
        return "PI=" + Math.PI + "_373=" + 373.ToString() + true.ToString() + short.MaxValue.ToString() + 'z';
    }

    [Benchmark]
    public string StringJoin()
    {
        return string.Join(' ', "PI=", Math.PI, "_373=", 373, true, short.MaxValue, 'z');
    }

    [Benchmark]
    public string StringConcat()
    {
        return string.Concat("PI=", Math.PI, "_373=", 373, true, short.MaxValue, 'z');
    }

    [Benchmark]
    public string StringReplace()
    {
        return "PI= 3,141592653589793 _373= 373 True 32767 z"
            .Replace("373", "5428")
            .Replace("St Paul", "HOT")
            .Replace("z", "LAST");
    }

    [Benchmark]
    public StringBuilder StringBuilderAppend()
    {
        return new StringBuilder()
            .Append("PI=")
            .Append(Math.PI)
            .Append("_373=")
            .Append(373)
            .Append(true)
            .Append(short.MaxValue)
            .Append('z');
    }

    [Benchmark]
    public StringBuilder StringBuilderAppend64()
    {
        return new StringBuilder(64)
            .Append("PI=")
            .Append(Math.PI)
            .Append("_373=")
            .Append(373)
            .Append(true)
            .Append(short.MaxValue)
            .Append('z');
    }

    [Benchmark]
    public StringBuilder StringBuilderReplace()
    {
        return new StringBuilder("PI= 3,141592653589793 _373= 373 True 32767 z")
            .Replace("373", "5428")
            .Replace("St Paul", "HOT")
            .Replace("z", "LAST")
            .Replace("5", "")
            .Replace("´6", "9");
    }

    [Benchmark]
    public MutString MutStringAppend()
    {
        return new MutString()
            .Append("PI=")
            .Append(Math.PI)
            .Append("_373=")
            .Append(373)
            .Append(true)
            .Append(short.MaxValue)
            .Append('z');
    }

    [Benchmark]
    public MutString MutStringAppend64()
    {
        return new MutString(64)
            .Append("PI=")
            .Append(Math.PI)
            .Append("_373=")
            .Append(373)
            .Append(true)
            .Append(short.MaxValue)
            .Append('z');
    }

    [Benchmark]
    public MutString MutStringReplace()
    {
        return new MutString("PI= 3,141592653589793 _373= 373 True 32767 z")
            .Replace("373", "5428")
            .Replace("St Paul", "HOT")
            .Replace("z", "LAST")
            .Replace("5", "")
            .Replace("´6", "9");
    }
    #endregion
#endif

#if LargeTest
    #region BIGString

    private readonly static string str1 = new string('a', 1000);
    private readonly static string str2 = new string('b', 1000);
    private readonly static string str3 = new string('c', 1000);
    private readonly static string str4 = new string('d', 1000);

    [Benchmark]
    public string LargeStringInterpolated()
    {
        return $"{str1} {str2}{str3}{str4}";
    }

    [Benchmark]
    public string LargeStringPlusOperation()
    {
        return str1 + str2 + str3 + str4;
    }

    [Benchmark]
    public string LargeStringConcat()
    {
        return string.Concat(str1, str2, str3, str4);
    }

    [Benchmark]
    public StringBuilder LargeStringBuilderAppend()
    {
        return new StringBuilder(1)
            .Append(str1)
            .Append(str2)
            .Append(str3)
            .Append(str4);
    }

    [Benchmark]
    public MutString LargeMutStringAppend()
    {
        return new MutString(1)
            .Append(str1)
            .Append(str2)
            .Append(str3)
            .Append(str4);
    }
    #endregion
#endif

#if PrimitiveTest
    #region PrimativeTypes
    [Benchmark]
    public string PrimitiveStringInterpolated()
    {
        return $"{char.MaxValue} {Int16.MaxValue}{Int32.MaxValue}{Int64.MaxValue}{DateTime.MaxValue}{double.MaxValue}{decimal.MaxValue}{float.MaxValue}{true}{byte.MaxValue}{sbyte.MaxValue}";
    }

    [Benchmark]
    public string PrimitiveStringPlusOperation()
    {
        return char.MaxValue + Int16.MaxValue.ToString() + Int32.MaxValue.ToString() + Int64.MaxValue.ToString() + DateTime.MaxValue.ToString() + double.MaxValue.ToString() + decimal.MaxValue.ToString() + float.MaxValue.ToString() + true.ToString() + byte.MaxValue.ToString() + sbyte.MaxValue.ToString();
    }

    [Benchmark]
    public string PrimitiveStringConcat()
    {
        return string.Concat(char.MaxValue, Int16.MaxValue, Int32.MaxValue, Int64.MaxValue, DateTime.MaxValue, double.MaxValue, float.MaxValue, true, byte.MaxValue, sbyte.MaxValue);
    }

    [Benchmark]
    public StringBuilder PrimitiveStringBuilder()
    {
        return new StringBuilder(64)
            .Append(char.MaxValue)
            .Append(Int16.MaxValue)
            .Append(Int32.MaxValue)
            .Append(Int64.MaxValue)
            .Append(DateTime.MaxValue)
            .Append(double.MaxValue)
            .Append(float.MaxValue)
            .Append(true)
            .Append(byte.MaxValue)
            .Append(sbyte.MaxValue);
    }

    [Benchmark]
    public MutString PrimitiveMutString()
    {
        return new MutString(64)
        .Append(char.MaxValue)
        .Append(Int16.MaxValue)
        .Append(Int32.MaxValue)
        .Append(Int64.MaxValue)
        .Append(DateTime.MaxValue)
        .Append(double.MaxValue)
        .Append(float.MaxValue)
        .Append(true)
        .Append(byte.MaxValue)
        .Append(sbyte.MaxValue);
    }
    #endregion
#endif
}