﻿using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celezt.String;
using BenchmarkDotNet.Order;

[MemoryDiagnoser, Orderer(SummaryOrderPolicy.FastestToSlowest), RankColumn]
public class MutStringBenchmark
{
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
    public string StringBuilderAppend()
    {
        return new StringBuilder()
            .Append("PI=")
            .Append(Math.PI)
            .Append("_373=")
            .Append(373)
            .Append(true)
            .Append(short.MaxValue)
            .Append('z')
            .ToString();
    }

    [Benchmark]
    public string StringBuilderAppend128()
    {
        return new StringBuilder(128)
            .Append("PI=")
            .Append(Math.PI)
            .Append("_373=")
            .Append(373)
            .Append(true)
            .Append(short.MaxValue)
            .Append('z')
            .ToString();
    }

    [Benchmark]
    public string MutStringAppend()
    {
        var ms = new MutString();

        ms.Append("PI=");
        ms.Append(Math.PI);
        ms.Append("_373=");
        ms.Append(373);
        ms.Append(true);
        ms.Append(short.MaxValue);
        ms.Append('z');

        return ms.ToString();
    }

    [Benchmark]
    public string MutStringAppend128()
    {
        var ms = new MutString(128);

        ms.Append("PI=");
        ms.Append(Math.PI);
        ms.Append("_373=");
        ms.Append(373);
        ms.Append(true);
        ms.Append(short.MaxValue);
        ms.Append('z');

        return ms.ToString();
    }
    #endregion

    //#region BIGString

    //private readonly static string str1 = new string('a', 1000);
    //private readonly static string str2 = new string('b', 1000);
    //private readonly static string str3 = new string('c', 1000);
    //private readonly static string str4 = new string('d', 1000);

    //[Benchmark]
    //public string LargeStringInterpolated()
    //{
    //    string str = $"{str1} {str2}{str3}{str4}";
    //    return str.Replace("c", "z");
    //}

    //[Benchmark]
    //public string LargeStringAdded()
    //{
    //    string str = str1 + str2 + str3 + str4;
    //    return str.Replace("c", "z");
    //}

    //[Benchmark]
    //public string LargeStringConcat()
    //{
    //    string str = string.Concat(str1, str2, str3, str4);
    //    return str.Replace("c", "z");
    //}


    //[Benchmark]
    //public string LargeStringBuilder()
    //{
    //    System.Text.StringBuilder m_strBuilder = new System.Text.StringBuilder(1);

    //    m_strBuilder.Append(str1)
    //        .Append(str2)
    //        .Append(str3)
    //        .Append(str4);

    //    return m_strBuilder.ToString();
    //}


    //[Benchmark]
    //public string LargeMutString()
    //{
    //    MutString mutString = new MutString(1);

    //    mutString.Append(str1);
    //    mutString.Append(str2);
    //    mutString.Append(str3);
    //    mutString.Append(str4);

    //    return mutString.ToString();
    //}
    //#endregion

    //#region BIGArray

    //private readonly static char[] _bigArray1;
    //private readonly static char[] _bigArray2;
    //private readonly static char[] _bigArray3;

    //static MutStringBenchmark()
    //{
    //    _bigArray1 = new char[char.MaxValue];
    //    _bigArray2 = new char[char.MaxValue];
    //    _bigArray3 = new char[char.MaxValue];
    //    for (var i = 0; i < char.MaxValue; i++)
    //    {
    //        _bigArray1[i] = (char)i;
    //        _bigArray2[i] = (char)i;
    //        _bigArray3[i] = (char)i;
    //    }
    //}


    //[Benchmark]
    //public string LargeArrayStringBuilder()
    //{
    //    System.Text.StringBuilder m_strBuilder = new System.Text.StringBuilder(1);

    //    m_strBuilder.Append(_bigArray1).Append(_bigArray2).Append(_bigArray3);

    //    return m_strBuilder.ToString();
    //}


    //[Benchmark]
    //public string LargeArrayMutString()
    //{
    //    MutString mutString = new MutString(1);

    //    mutString.Append(_bigArray1);
    //    mutString.Append(_bigArray2);
    //    mutString.Append(_bigArray3);

    //    return mutString.ToString();
    //}


    //#endregion

    //#region PrimativeTypes


    //[Benchmark]
    //public string PrimitiveStringInterpolated()
    //{
    //    string str = $"{char.MaxValue} {Int16.MaxValue}{Int32.MaxValue}{Int64.MaxValue}{DateTime.MaxValue}{double.MaxValue}{decimal.MaxValue}{float.MaxValue}{true}{byte.MaxValue}{sbyte.MaxValue}";
    //    return str;
    //}

    //[Benchmark]
    //public string PrimitiveStringAdded()
    //{
    //    string str = char.MaxValue + Int16.MaxValue.ToString() + Int32.MaxValue.ToString() + Int64.MaxValue.ToString() + DateTime.MaxValue.ToString() + double.MaxValue.ToString() + decimal.MaxValue.ToString() + float.MaxValue.ToString() + true.ToString() + byte.MaxValue.ToString() + sbyte.MaxValue.ToString();
    //    return str;
    //}

    //[Benchmark]
    //public string PrimitiveStringConcat()
    //{
    //    string str = string.Concat(char.MaxValue, Int16.MaxValue, Int32.MaxValue, Int64.MaxValue, DateTime.MaxValue, double.MaxValue, float.MaxValue, true, byte.MaxValue, sbyte.MaxValue);
    //    return str.Replace("c", "z");
    //}

    //[Benchmark]
    //public string PrimitiveStringBuilder()
    //{
    //    StringBuilder m_strBuilder = new StringBuilder(64);

    //    m_strBuilder
    //        .Append(char.MaxValue)
    //        .Append(Int16.MaxValue)
    //        .Append(Int32.MaxValue)
    //        .Append(Int64.MaxValue)
    //        .Append(DateTime.MaxValue)
    //        .Append(double.MaxValue)
    //        .Append(float.MaxValue)
    //        .Append(true)
    //        .Append(byte.MaxValue)
    //        .Append(sbyte.MaxValue);

    //    return m_strBuilder.ToString();
    //}

    //[Benchmark]
    //public string PrimitiveMutString()
    //{
    //    MutString mutString = new MutString(64);

    //    mutString.Append(char.MaxValue);
    //    mutString.Append(Int16.MaxValue);
    //    mutString.Append(Int32.MaxValue);
    //    mutString.Append(Int64.MaxValue);
    //    mutString.Append(DateTime.MaxValue);
    //    mutString.Append(double.MaxValue);
    //    mutString.Append(float.MaxValue);
    //    mutString.Append(true);
    //    mutString.Append(byte.MaxValue);
    //    mutString.Append(sbyte.MaxValue);

    //    return mutString.ToString();
    //}


    //#endregion
}