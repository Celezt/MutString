using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using System.Threading;
using System.Globalization;

namespace Celezt.Text.Tests;

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
    public void TestLength()
    {
        var ms = new MutString();

        Assert.IsTrue(ms.Length == 0);

        ms.Append("Hello");
        Assert.IsTrue(ms.Length == 5);

        ms.Length = 2;
        Assert.IsTrue(ms.Length == 2);
        Assert.AreEqual("He", ms.ToString());

        Throws<ArgumentOutOfRangeException>(() =>
        {
            ms.Length = -1;
        });
        Throws<ArgumentOutOfRangeException>(() =>
        {
            ms.Length = 17;
        });
    }

    [TestMethod]
    public void TestIsEmpty()
    {
        var ms = new MutString();

        Assert.IsTrue(ms.IsEmpty);

        ms.Append("Hello");
        Assert.IsFalse(ms.IsEmpty);
    }

    [TestMethod]
    public void TestClear()
    {
        var ms = new MutString();

        Assert.IsTrue(ms.IsEmpty);

        ms.Append("Hello");
        Assert.IsFalse(ms.IsEmpty);
        Assert.AreEqual("Hello", ms.ToString());

        ms.Clear();
        Assert.IsTrue(ms.IsEmpty);
        Assert.AreEqual("", ms.ToString());
    }

    [TestMethod]
    public void TestCreate()
    {
        var ms = MutString.Create();

        ms.Append("Hello");
        Assert.IsFalse(ms.IsEmpty);
        Assert.AreEqual("Hello", ms.ToString());

        ms = MutString.Create(32);
        ms.Append("Hello");
        Assert.IsFalse(ms.IsEmpty);
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

        Throws<IndexOutOfRangeException>(() =>
        {
            var c = ms[5];
        });
        Throws<ArgumentOutOfRangeException>(() =>
        {
            ms[5] = 'c';
        });

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

        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        ms.Clear();
        var date = DateTime.Now;
        ms.Append<DateTime>(date);
        Assert.AreEqual(date.ToString(), (string)ms);

        ms.Clear();
        ms.Append<float>(3.14f);
        Assert.AreEqual("3.14", (string)ms);

        ms.Clear();
        ms.Append<char[]>(new char[] { 'H', 'e', 'l', 'l', 'o' });
        Assert.AreEqual("Hello", (string)ms);
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
    public void TestInsert()
    {
        var ms = MutString.Create();
        Assert.IsTrue(ms.Length == 0);
        ms.Insert(0, (string)null);
        Assert.IsTrue(ms.Length == 0);
        ms.Insert(0, string.Empty);
        Assert.IsTrue(ms.Length == 0);

        ms.Insert(0, (char[])null);
        Assert.IsTrue(ms.Length == 0);

        ms.Clear();
        Assert.IsTrue(ms.Length == 0);
        ms.Insert(0, true);
        Assert.AreEqual("True", ms.ToString());

        ms.Clear();
        ms.Insert(0, false);
        Assert.AreEqual("False", ms.ToString());


        ms.Clear();
        ms.Insert(0, "a");
        Assert.AreEqual("a", ms.ToString());

        ms.Clear();
        ms.Insert(0, "ab");
        Assert.AreEqual("ab", ms.ToString());


        ms.Clear();
        ms.Insert(0, "abc");
        Assert.AreEqual("abc", ms.ToString());

        ms.Clear();
        ms.Insert(0, "abcd");
        Assert.AreEqual("abcd", ms.ToString());

        ms.Clear();
        ms.Insert(0, 0);
        Assert.AreEqual("0", ms.ToString());

        ms.Clear();
        ms.Insert(0, Convert.ToByte('a'));
        Assert.AreEqual(Convert.ToByte('a').ToString(), ms.ToString());

        ms.Clear();
        ms.Insert(0, (sbyte)Convert.ToByte('a'));
        Assert.AreEqual(((sbyte)Convert.ToByte('a')).ToString(), ms.ToString());

        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        var dt = DateTime.Now;
        ms.Clear();
        ms.Insert(0, dt);
        Assert.AreEqual(dt.ToString(), ms.ToString());

        ms.Clear();
        ms.Insert(0, (float)1.1234);
        Assert.AreEqual("1.1234", ms.ToString());

        ms.Clear();
        ms.Insert(0, 1000);
        Assert.AreEqual("1000", ms.ToString());

        ms.Clear();
        ms.Insert(0, -1000);
        Assert.AreEqual("-1000", ms.ToString());

        ms.Clear();
        ms.Insert(0, (decimal)0);
        Assert.AreEqual("0", ms.ToString());

        ms.Clear();
        ms.Insert(0, (decimal)1000);
        Assert.AreEqual("1000", ms.ToString());

        ms.Clear();
        ms.Insert(0, (decimal)-1000.123);
        Assert.AreEqual("-1000.123", ms.ToString());


        ms.Clear();
        ms.Insert(0, (decimal)100000.123456789);
        Assert.AreEqual("100000.123456789", ms.ToString());

        ms.Clear();
        ms.Insert(0, (double)100000.123456789);
        Assert.AreEqual("100000.123456789", ms.ToString());

        ms.Clear();
        ms.Insert(0, (double)-123412342.123);
        Assert.AreEqual("-123412342.123", ms.ToString());


        ms.Clear();
        ms.Insert(0, (long)0);
        Assert.AreEqual("0", ms.ToString());

        ms.Clear();
        ms.Insert(0, (long)1000);
        Assert.AreEqual("1000", ms.ToString());

        ms.Clear();
        ms.Insert(0, (long)-1000);
        Assert.AreEqual("-1000", ms.ToString());

        ms.Clear();
        ms.Insert(0, (short)1000);
        Assert.AreEqual("1000", ms.ToString());

        ms.Clear();
        ms.Insert(0, (ulong)1000);
        Assert.AreEqual("1000", ms.ToString());

        ms.Clear();
        ms.Insert(0, (uint)1000);
        Assert.AreEqual("1000", ms.ToString());

        ms.Clear();
        ms.Insert(0, (object)"Hello");
        Assert.AreEqual("Hello", ms.ToString());

        ms.Clear();
        ms.Insert(0, (object)null);
        Assert.AreEqual("", ms.ToString());

        ms.Clear();
        ms.Insert(0, new char[] { 'h', 'i' });
        Assert.AreEqual("hi", ms.ToString());

        ms = MutString.Create();
        for (var i = 0; i < 1000; i++)
        {
            ms.Insert(i, 'a');
        }
        Assert.IsTrue(ms.Length == 1000);

        ms = new MutString("Hello World");
        ms.Insert(0, "New ");
        Assert.AreEqual("New Hello World", (string)ms);

        ms.Insert(ms.Length, "!");
        Assert.AreEqual("New Hello World!", (string)ms);

        ms.Insert(3, " Mighty");
        Assert.AreEqual("New Mighty Hello World!", (string)ms);

        Assert.AreEqual(64, ms.Capacity);
        ms.Insert(0, "This is a really long line of text that you are reading right now! ");
        Assert.AreEqual("This is a really long line of text that you are reading right now! New Mighty Hello World!", (string)ms);
        Assert.AreEqual(128, ms.Capacity);

        ms.Insert(0, 69);
        Assert.AreEqual("69This is a really long line of text that you are reading right now! New Mighty Hello World!", (string)ms);

        ms.Insert(ms.Length, 420);
        Assert.AreEqual("69This is a really long line of text that you are reading right now! New Mighty Hello World!420", (string)ms);

        ms.Insert(68, 1111);
        Assert.AreEqual("69This is a really long line of text that you are reading right now!1111 New Mighty Hello World!420", (string)ms);

        Throws<ArgumentOutOfRangeException>(() =>
        {
            ms.Insert(-1, "Something");
        });

        Throws<ArgumentOutOfRangeException>(() =>
        {
            ms.Insert(ms.Length + 1, "Something");
        });
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
        Throws<ArgumentNullException>(() =>
        {
            ms.Replace("", "justin");
        });

        Throws<ArgumentNullException>(() =>
        {
            ms.Replace(null, "justin");
        });
        Assert.AreEqual("abc", ms.ToString());

        ms.Replace("z", "justin");
        Assert.AreEqual("abc", ms.ToString());

        ms.Replace("z", "");
        Assert.AreEqual("abc", ms.ToString());

        ms.Replace("abc", "");
        Assert.AreEqual("", ms.ToString());

        ms.Replace("abc", null);
        Assert.AreEqual("", ms.ToString());

        ms.Clear();
        ms.Append("fffffffffffffff");
        ms.Replace("f", "");
        Assert.AreEqual("", ms.ToString());

        ms.Append("123BCabc123BCdefg123BC");
        ms.Replace('2', '9');
        Assert.AreEqual("193BCabc193BCdefg193BC", ms.ToString());

        ms.Clear();
        ms.Append("88888888");
        ms.Replace('8', '1');
        Assert.AreEqual("11111111", ms.ToString());
    }

    [TestMethod]
    public void TestSet()
    {
        var ms = MutString.Create();
        ms.Append("Hello World");
        ms.Set("Hi");
        Assert.IsFalse(ms.IsEmpty);
        Assert.AreEqual("Hi", ms.ToString());


        ms.Set("123", "45");
        Assert.IsFalse(ms.IsEmpty);
        Assert.AreEqual("12345", ms.ToString());

        ms.Set(null, "",123);
        Assert.IsFalse(ms.IsEmpty);
        Assert.AreEqual("123", ms.ToString());

        ms.Set(null, "", null);
        Assert.IsTrue(ms.IsEmpty);
        Assert.AreEqual("", ms.ToString());
    }

    [TestMethod]
    public void TestClone()
    {
        var ms = MutString.Create("Hello");
        var ms1 = ms.Clone();
        ms1.Append(" beautiful world!");

        Assert.AreEqual("Hello", (string)ms);
        Assert.AreEqual("Hello beautiful world!", (string)ms1);

        Assert.AreEqual(5, ms.Length);
        Assert.AreEqual(22, ms1.Length);

        Assert.AreEqual(16, ms.Capacity);
        Assert.AreEqual(64, ms1.Capacity);

        ms1.Set("No");
        Assert.AreEqual("Hello", (string)ms);
        Assert.AreEqual("No", (string)ms1);

        Assert.AreEqual(5, ms.Length);
        Assert.AreEqual(2, ms1.Length);

        Assert.AreEqual(16, ms.Capacity);
        Assert.AreEqual(64, ms1.Capacity);
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
    public void TestImplicitCast()
    {
        var ms = new MutString("Hello");
        var chr = new char[] { 'H', 'e', 'l', 'l', 'o' };

        Assert.IsTrue(MemoryExtensions.Equals("Hello".AsSpan(), ms, StringComparison.Ordinal));
        Assert.IsTrue(MemoryExtensions.Equals(chr.AsSpan(), ms, StringComparison.Ordinal));
    }

    [TestMethod]
    public void TestExplicitCast()
    {
        var ms = new MutString("Hello");
        var chr = new char[] { 'H', 'e', 'l', 'l', 'o' };

        Assert.AreEqual("Hello", (string)ms);
        Assert.IsTrue(MemoryExtensions.SequenceEqual(chr.AsSpan(), (char[])ms));
        Assert.IsTrue(MemoryExtensions.Equals("Hello", (Span<char>)ms, StringComparison.Ordinal));
        Assert.IsTrue(MemoryExtensions.Equals("Hello", (ReadOnlySpan<char>)ms, StringComparison.Ordinal));

        Assert.AreEqual(ms, (MutString)"Hello");
        Assert.AreEqual(ms, (MutString)("Hello".AsSpan()));
        Assert.AreEqual(ms, (MutString)chr);
        Assert.AreEqual(ms, (MutString)(chr.AsSpan()));
    }

    [TestMethod]
    public void TestComparisonOperators()
    {
        var ms = MutString.Create("Hello");
        var ms2 = ms;
        ref var ms3 = ref ms;

        var chr = new char[] { 'H', 'e', 'l', 'l', 'o' };

        Assert.IsTrue(ms == ms2);
        Assert.IsTrue(ms == ms3);
        Assert.IsTrue(ms == new MutString("Hello"));
        Assert.IsTrue(ms == "Hello");
        Assert.IsTrue("Hello" == ms);
        Assert.IsTrue(ms == "Hello".AsSpan());
        Assert.IsTrue("Hello".AsSpan() == ms);
        Assert.IsTrue(ms == chr);
        Assert.IsTrue(chr == ms);
        Assert.IsTrue(ms == chr.AsSpan());
        Assert.IsTrue(chr.AsSpan() == ms);

        Assert.IsFalse(ms != ms2);
        Assert.IsFalse(ms != ms3);
        Assert.IsFalse(ms != new MutString("Hello"));
        Assert.IsFalse(ms != "Hello");
        Assert.IsFalse("Hello" != ms);
        Assert.IsFalse(ms != "Hello".AsSpan());
        Assert.IsFalse("Hello".AsSpan() != ms);
        Assert.IsFalse(ms != chr);
        Assert.IsFalse(chr != ms);
        Assert.IsFalse(ms != chr.AsSpan());
        Assert.IsFalse(chr.AsSpan() != ms);
    }

    [TestMethod]
    public void TestEnumerator()
    {
        var ms = MutString.Create("Hello World");
        Span<char> span = stackalloc char[11];
        int length = 0;

        foreach (char c in ms)
            span[length++] = c;

        Assert.IsTrue(ms == span);
    }

    [TestMethod]
    public void TestComparable()
    {
        var ms = MutString.Create("Hello World");

        Assert.IsTrue(ms.CompareTo((MutString)"Hello World", StringComparison.Ordinal) == 0);
        Assert.IsTrue(ms.CompareTo((MutString)"Gello World", StringComparison.Ordinal) > 0);
        Assert.IsTrue(ms.CompareTo((MutString)"Iello World", StringComparison.Ordinal) < 0);

        Assert.IsTrue(ms.CompareTo((MutString)"hello world", StringComparison.OrdinalIgnoreCase) == 0);
        Assert.IsTrue(ms.CompareTo((MutString)"gello world", StringComparison.OrdinalIgnoreCase) > 0);
        Assert.IsTrue(ms.CompareTo((MutString)"iello world", StringComparison.OrdinalIgnoreCase) < 0);
    }

    [TestMethod]
    public void TestRemove()
    {
        var ms = MutString.Create("Hello World");

        ms.Remove(4);
        Assert.AreEqual("Hello", (string)ms);

        ms.Append("New Wonderful World!");
        ms.Remove(0, 5);
        Assert.AreEqual("New Wonderful World!", (string)ms);

        ms.Remove(13, 7);
        Assert.AreEqual("New Wonderful", (string)ms);

        ms.Remove(4, 1);
        Assert.AreEqual("New onderful", (string)ms);

        ms.Remove(1, 3);
        Assert.AreEqual("Nonderful", (string)ms);

        Throws<ArgumentOutOfRangeException>(() =>
        {
            ms.Remove(-1);
        });

        Throws<ArgumentOutOfRangeException>(() =>
        {
            ms.Remove(9);
        });

        Throws<ArgumentOutOfRangeException>(() =>
        {
            ms.Remove(1, 9);
        });
    }

    [TestMethod]
    public void TestIndexOf()
    {
        var ms = MutString.Create("Hello World");

        Assert.AreEqual(0, ms.IndexOf('H'));
        Assert.AreEqual(10, ms.IndexOf('d'));
        Assert.AreEqual(5, ms.IndexOf(' '));
        Assert.AreEqual(7, ms.IndexOf('o', 5));
        Assert.AreEqual(-1, ms.IndexOf('o', 0, 3));
        Assert.AreEqual(-1, ms.IndexOf('o', 5, 2));
        Assert.AreEqual(-1, ms.IndexOf('g'));

        Assert.AreEqual(0, ms.IndexOf("Hello"));
        Assert.AreEqual(6, ms.IndexOf("World"));
        Assert.AreEqual(3, ms.IndexOf("l", 3));
        Assert.AreEqual(-1, ms.IndexOf("Something"));
        Assert.AreEqual(6, ms.IndexOf(new char[] { 'W', 'o' }));

        Throws<ArgumentOutOfRangeException>(() =>
        {
            ms.IndexOf('H', -1);
        });

        Throws<ArgumentOutOfRangeException>(() =>
        {
            ms.IndexOf('H', 0, -1);
        });

        Throws<ArgumentOutOfRangeException>(() =>
        {
            ms.IndexOf('H', 0, 12);
        });

        Throws<ArgumentOutOfRangeException>(() =>
        {
            ms.IndexOf('H', 12, 0);
        });
    }

    [TestMethod]
    public void TestContains()
    {
        var ms = MutString.Create("Hello World");

        Assert.IsTrue(ms.Contains('o'));
        Assert.IsTrue(ms.Contains("o"));
        Assert.IsTrue(ms.Contains("Hello"));
        Assert.IsTrue(ms.Contains("World"));

        Assert.IsFalse(ms.Contains('g'));
        Assert.IsFalse(ms.Contains("Something"));
    }

    [TestMethod]
    public void TestTrim()
    {
        var ms = MutString.Create("   Hello World   ");

        ms.TrimStart();
        Assert.AreEqual("Hello World   ", (string)ms);

        ms.TrimEnd();
        Assert.AreEqual("Hello World", (string)ms);

        ms.Set("   Hello World   ");
        ms.Trim();
        Assert.AreEqual("Hello World", (string)ms);
    }

    public static void Throws<T>(Action task) where T : Exception
    {
        try
        {
            task();
        }
        catch (Exception e)
        {
            Assert.IsTrue(typeof(T).IsAssignableTo(e.GetType()));
            return;
        }

        if (typeof(T).Equals(typeof(Exception)))
            Assert.Fail("Expected exception but no exception was thrown.");
        else
            Assert.Fail($"Expected exception of type {typeof(T)} but no exception was thrown.");
    }
}
