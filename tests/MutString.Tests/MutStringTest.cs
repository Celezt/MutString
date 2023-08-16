using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using System.Threading;
using System.Globalization;

namespace Celezt.String.Tests;

[ExcludeFromCodeCoverage]
[TestClass]
public class MutStringTest
{
    [TestMethod]
    public void TestMutString()
    {
        var ms = new MutString();
        ms = new MutString(0);
        Assert.AreEqual("", ms.ToString());
        ms = new MutString(-32);
        Assert.AreEqual("", ms.ToString());
        ms = new MutString((string)null);
        Assert.AreEqual("", ms.ToString());
        ms = new MutString((char[])null);
        Assert.AreEqual("", ms.ToString());
        ms = new MutString(ReadOnlySpan<char>.Empty);
        Assert.AreEqual("", ms.ToString());
        ms = new MutString("");
        Assert.AreEqual("", ms.ToString());
        ms = new MutString("Hello");
        Assert.AreEqual("Hello", ms.ToString());
    }

    [TestMethod]
    public void TestCorruptStrings()
    {
        var chars = new char[char.MaxValue];
        for(var i = char.MinValue; i < chars.Length; i++)
            chars[i] = i;

        var str = new string(chars);
        var ms = new MutString();
        ms.Append(chars);

        Assert.AreEqual(str, ms.ToString());

        Assert.AreEqual(str.Length, ms.Length);
        Assert.AreEqual(ms.Length, ms.ToString().Length);


        ms.Clear();
        var singleByteString = "s";
        var doubleByteString = "ß";
        var quadByteString = "𝟘";

        ms.Append(singleByteString);
        Assert.AreEqual(singleByteString, ms.ToString());

        ms.Append(doubleByteString);
        Assert.AreEqual(singleByteString + doubleByteString, ms.ToString());

        ms.Append(quadByteString);
        Assert.AreEqual(singleByteString + doubleByteString + quadByteString, ms.ToString());
    }

    [TestMethod]
    public void TestCapacity()
    {
        var ms = new MutString();

        Assert.AreEqual(16, ms.Capacity);

        ms.Append("Hello");
        Assert.AreEqual(16, ms.Capacity);

        ms.Append("Hello beautiful world!");
        Assert.AreEqual(64, ms.Capacity);
    }

    [TestMethod]
    public void TestMutStringThreaded()
    {
        static string GetString(char c) => new string(c, 1000);

        var list = new System.Collections.Concurrent.ConcurrentDictionary<int,string>();
        System.Threading.Tasks.Parallel.For(0, 1000, (i) =>
        {
            var fs = new MutString(1);
            fs.Append(GetString((char)(48 + (i % 10))));
            list.TryAdd(i, fs.ToString());
        });

        var dic = list.ToArray();
        for(var i = 0; i < 1000; i++)
        {
            var key = dic[i].Key;
            var value = dic[i].Value;
            var str = GetString((char)(48 + (key % 10)));
            if (string.Compare(str, value) != 0)
            {
                Assert.Fail("Not thread safe");
            }
        }
    }

    [TestMethod]
    public void TestLength()
    {
        var ms = new MutString();
        Assert.IsTrue(ms.Length == 0);
        ms.Append("Hello");
        Assert.IsTrue(ms.Length == 5);
    }

    [TestMethod]
    public void TestIsEmpty()
    {
        var ms = new MutString();
        Assert.IsTrue(ms.IsEmpty());
        ms.Append("Hello");
        Assert.IsFalse(ms.IsEmpty());
    }

    [TestMethod]
    public void TestClear()
    {
        var ms = new MutString();
        Assert.IsTrue(ms.IsEmpty());
        ms.Append("Hello");
        Assert.IsFalse(ms.IsEmpty());
        Assert.AreEqual("Hello", ms.ToString());
        ms.Clear();
        Assert.IsTrue(ms.IsEmpty());
        Assert.AreEqual("", ms.ToString());
    }

    [TestMethod]
    public void TestCreate()
    {
        var ms = MutString.Create();
        ms.Append("Hello");
        Assert.IsFalse(ms.IsEmpty());
        Assert.AreEqual("Hello", ms.ToString());
        ms = MutString.Create(32);
        ms.Append("Hello");
        Assert.IsFalse(ms.IsEmpty());
        Assert.AreEqual("Hello", ms.ToString());
    }

    [TestMethod]
    public void TestEquals()
    {
        var ms = MutString.Create();
        ms.Append("Hello");

        var ms1 = MutString.Create();
        ms1.Append("Hello");

        Assert.IsTrue(ms1.Equals(ms));

        ms1 = MutString.Create();
        ms1.Append("hello");
        Assert.IsFalse(ms1.Equals(ms));


        ms1.Clear();
        ms1.Append("Hello");
        Assert.IsTrue(ms1.Equals(ms));

        Assert.IsFalse(ms1.Equals((string)null));

        var sb2 = ms;
        Assert.IsTrue(sb2.Equals(ms));

        ms1.Clear();
        ms1.Append("Hi");
        Assert.IsFalse(ms1.Equals(ms));

        Assert.IsFalse(ms1.Equals(new object()));
    }

    [TestMethod]
    public void TestGetHashCode()
    {
        var ms = MutString.Create();
        ms.Append("Hello");

        var ms1 = MutString.Create();
        ms1.Append("Hello");

        Assert.IsTrue(ms1.GetHashCode() == ms.GetHashCode());

        ms1 = MutString.Create();
        ms1.Append("hello");
        Assert.IsFalse(ms1.GetHashCode() == ms.GetHashCode());


        ms1.Clear();
        ms1.Append("Hello");
        Assert.IsTrue(ms1.GetHashCode() == ms.GetHashCode());


        var ms2 = ms;
        Assert.IsTrue(ms2.GetHashCode() == ms.GetHashCode());

        ms1.Clear();
        ms1.Append("Hi");
        Assert.IsFalse(ms1.GetHashCode() == ms.GetHashCode());

    }

    [TestMethod]
    public void TestIndex()
    {
        var ms = MutString.Create();
        ms.Append("abcd");
        try
        {
            var c = ms[5];
            Assert.Fail("Should have exception");
        }
        catch (Exception)
        {
            Assert.IsTrue(true);
        }

        try
        {
            ms[5] = 'c';
            Assert.Fail("Should have exception");
        }
        catch (Exception)
        {
            Assert.IsTrue(true);
        }


        Assert.AreEqual('b', ms[1]);
        ms[1] = 'z';
        Assert.AreEqual('z', ms[1]);
    }

    [TestMethod]
    public void TestAppendT()
    {
        var ms = MutString.Create();
        ms.Append<int>(null);
        Assert.AreEqual(0, ms.Length);
        ms.Append<string>("", null, "1");
        Assert.AreEqual(1, ms.Length);
        ms.Clear();
        ms.Append<int>(1, 2, 3, 4, 5);
        Assert.AreEqual(5, ms.Length);
        ms.Clear();

        ms.Append<int>(1);
        Assert.AreEqual(1, ms.Length);
    }

    [TestMethod]
    public void TestAppendObjects()
    {
        var ms = MutString.Create();
        ms.Append("Test", 'c', true, double.MaxValue, long.MaxValue,ulong.MaxValue,uint.MaxValue,null, int.MaxValue, short.MaxValue, decimal.MaxValue, double.MaxValue, float.MaxValue, DateTime.MinValue, sbyte.MinValue, byte.MinValue, new char[2] { 'c', 'b' });
        Assert.IsTrue(true);
    }

    [TestMethod]
    public void TestAppend()
    {
        var ms = MutString.Create();
        Assert.IsTrue(ms.Length == 0);
        ms.Append((string)null);
        Assert.IsTrue(ms.Length == 0);
        ms.Append(string.Empty);
        Assert.IsTrue(ms.Length == 0);

        ms.Append((char[])null);
        Assert.IsTrue(ms.Length == 0);

        ms.Clear();
        Assert.IsTrue(ms.Length == 0);
        ms.Append(true);
        Assert.AreEqual("True", ms.ToString());

        ms.Clear();
        ms.Append(false);
        Assert.AreEqual("False", ms.ToString());


        ms.Clear();
        ms.Append("a");
        Assert.AreEqual("a", ms.ToString());

        ms.Clear();
        ms.Append("ab");
        Assert.AreEqual("ab", ms.ToString());


        ms.Clear();
        ms.Append("abc");
        Assert.AreEqual("abc", ms.ToString());

        ms.Clear();
        ms.Append("abcd");
        Assert.AreEqual("abcd", ms.ToString());

        ms.Clear();
        ms.Append(0);
        Assert.AreEqual("0", ms.ToString());

        ms.Clear();
        ms.Append(Convert.ToByte('a'));
        Assert.AreEqual(Convert.ToByte('a').ToString(), ms.ToString());

        ms.Clear();
        ms.Append((sbyte)Convert.ToByte('a'));
        Assert.AreEqual(((sbyte)Convert.ToByte('a')).ToString(), ms.ToString());

        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        var dt = DateTime.Now;
        ms.Clear();
        ms.Append(dt);
        Assert.AreEqual(dt.ToString(), ms.ToString());

        ms.Clear();
        ms.Append((float)1.1234);
        Assert.AreEqual("1.1234", ms.ToString());

        ms.Clear();
        ms.Append(1000);
        Assert.AreEqual("1000", ms.ToString());

        ms.Clear();
        ms.Append(-1000);
        Assert.AreEqual("-1000", ms.ToString());

        ms.Clear();
        ms.Append((decimal)0);
        Assert.AreEqual("0", ms.ToString());

        ms.Clear();
        ms.Append((decimal)1000);
        Assert.AreEqual("1000", ms.ToString());

        ms.Clear();
        ms.Append((decimal)-1000.123);
        Assert.AreEqual("-1000.123", ms.ToString());


        ms.Clear();
        ms.Append((decimal)100000.123456789);
        Assert.AreEqual("100000.123456789", ms.ToString());

        ms.Clear();
        ms.Append((double)100000.123456789);
        Assert.AreEqual("100000.123456789", ms.ToString());

        ms.Clear();
        ms.Append((double)-123412342.123);
        Assert.AreEqual("-123412342.123", ms.ToString());


        ms.Clear();
        ms.Append((long)0);
        Assert.AreEqual("0", ms.ToString());

        ms.Clear();
        ms.Append((long)1000);
        Assert.AreEqual("1000", ms.ToString());
        ms.Clear();
        ms.Append((long)-1000);
        Assert.AreEqual("-1000", ms.ToString());

        ms.Clear();
        ms.Append((short)1000);
        Assert.AreEqual("1000", ms.ToString());

        ms.Clear();
        ms.Append((ulong)1000);
        Assert.AreEqual("1000", ms.ToString());

        ms.Clear();
        ms.Append((uint)1000);
        Assert.AreEqual("1000", ms.ToString());

        ms.Clear();
        ms.Append((object)"Hello");
        Assert.AreEqual("Hello", ms.ToString());

        ms.Clear();
        ms.Append((object)null);
        Assert.AreEqual("", ms.ToString());

        ms.Clear();
        ms.Append(new char[] { 'h', 'i' });
        Assert.AreEqual("hi", ms.ToString());

        ms = MutString.Create();
        for(var i = 0; i < 1000; i++)
        {
            ms.Append('a');
        }
        Assert.IsTrue(ms.Length == 1000);
    }

    [TestMethod]
    public void TestAppendline()
    {
        var ms = MutString.Create();
        Assert.IsTrue(ms.Length == 0);
        ms.AppendLine();
        Assert.IsTrue(ms.Length == Environment.NewLine.Length);

        ms.Clear();
        ms.AppendLine("Hi");
        Assert.IsTrue(ms.Length == Environment.NewLine.Length + 2);

        ms.Clear();
        ms.AppendLine("");
        Assert.IsTrue(ms.Length == Environment.NewLine.Length);

        ms.Clear();
        ms.AppendLine(null);
        Assert.IsTrue(ms.Length == Environment.NewLine.Length);
    }

    [TestMethod]
    public void TestReplace()
    {
        var ms = MutString.Create();
        ms.Append("ABCabcABCdefgABC");
        ms.Replace("ABC", "123");
        Assert.AreEqual("123abc123defg123", ms.ToString());

        ms.Clear();
        ms.Append("ABCabcABCdefgABC");
        ms.Replace("A", "123");
        Assert.AreEqual("123BCabc123BCdefg123BC", ms.ToString());

        ms.Replace("1", "one");
        Assert.AreEqual("one23BCabcone23BCdefgone23BC", ms.ToString());

        ms.Replace("2", "");
        Assert.AreEqual("one3BCabcone3BCdefgone3BC", ms.ToString());

        ms.Replace("3", "4");
        Assert.AreEqual("one4BCabcone4BCdefgone4BC", ms.ToString());


        ms.Replace("one", "1");
        Assert.AreEqual("14BCabc14BCdefg14BC", ms.ToString());

        ms.Replace("1", null);
        Assert.AreEqual("4BCabc4BCdefg4BC", ms.ToString());

        ms.Clear();
        ms.Append("abc");
        ms.Replace("", "justin");
        Assert.AreEqual("abc", ms.ToString());

        ms.Replace(null, "justin");
        Assert.AreEqual("abc", ms.ToString());

        ms.Replace("z", "justin");
        Assert.AreEqual("abc", ms.ToString());

        ms.Replace("z", "");
        Assert.AreEqual("abc", ms.ToString());

        ms.Replace("abc", "");
        Assert.AreEqual("", ms.ToString());

        ms.Replace("abc", null);
        Assert.AreEqual("", ms.ToString());
    }

    [TestMethod]
    public void TestSet()
    {
        var ms = MutString.Create();
        ms.Append("Hello World");
        ms.Set("Hi");
        Assert.IsFalse(ms.IsEmpty());
        Assert.AreEqual("Hi", ms.ToString());


        ms.Set("123", "45");
        Assert.IsFalse(ms.IsEmpty());
        Assert.AreEqual("12345", ms.ToString());

        ms.Set(null, "",123);
        Assert.IsFalse(ms.IsEmpty());
        Assert.AreEqual("123", ms.ToString());

        ms.Set(null, "", null);
        Assert.IsTrue(ms.IsEmpty());
        Assert.AreEqual("", ms.ToString());
    }

    [TestMethod]
    public void TestSpanAndMemory()
    {
        var ms = MutString.Create();

        ms.Set("Hello World");
        Assert.IsTrue(MemoryExtensions.Equals(ms.Span, "Hello World", StringComparison.Ordinal));
        Assert.IsTrue(MemoryExtensions.Equals(ms.Memory.Span, "Hello World", StringComparison.Ordinal));

        ms.Set("Hello");
        Assert.IsTrue(MemoryExtensions.Equals(ms.Span, "Hello", StringComparison.Ordinal));
        Assert.IsTrue(MemoryExtensions.Equals(ms.Memory.Span, "Hello", StringComparison.Ordinal));
    }

    [TestMethod]
    public void TextImplicitCast()
    {
        MutString Implicit(MutString mut) => mut;

        var ms = MutString.Create();

        Assert.AreEqual(new MutString("Hello"), Implicit("Hello"));
        Assert.AreEqual(new MutString("Hello"), Implicit("Hello".AsSpan()));
        Assert.AreEqual(new MutString("Hello"), Implicit(new char[] { 'H', 'e', 'l', 'l', 'o' }));
        Assert.AreEqual(new MutString("Hello"), Implicit((new char[] { 'H', 'e', 'l', 'l', 'o' }).AsSpan()));
    }
}
