using System.Globalization;
using System.Runtime.CompilerServices;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections;

namespace Celezt.String;

///<summary>
/// Mutable <see cref="string"/>.
///</summary>
#if !NETSTANDARD1_3
[Serializable]
#endif
public class MutString : IComparable, IComparable<MutString>, IEnumerable, IEnumerable<char>, IEquatable<MutString>
#if !NETSTANDARD1_3
    , ICloneable 
 #endif
{
    private const int DEFAULT_CAPACITY = 16;

    private readonly static CultureInfo _defaultCulture = CultureInfo.InvariantCulture;
    private readonly static ArrayPool<char> _arrayPool = ArrayPool<char>.Shared;
    private readonly static char[] _charNumbers = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
    private readonly static char[][] _bool = new char[2][]
    {
        new char[]{ 'F','a','l','s','e'},
        new char[]{ 'T', 'r','u','e' }
    };

    public int Length
    {
        get => _bufferPos;
        set
        {
            if (value > Capacity || value < 0)
                throw new ArgumentOutOfRangeException("value");

            _bufferPos = value;
        }
    }

    public int Capacity => _buffer.Length;

    public bool IsEmpty => _bufferPos == 0;

    public Memory<char> Memory => _buffer.AsMemory(0, _bufferPos);
    public Span<char> Span => _buffer.AsSpan(0, _bufferPos);
    public ReadOnlySpan<char> ReadOnlySpan => _buffer.AsSpan(0, _bufferPos);

    public char this[int index]
    {
        get
        {
            if (index > _bufferPos || index < 0)
                throw new IndexOutOfRangeException();

            return _buffer[index];
        }
        set
        {
            if (index > _bufferPos || index < 0)
                throw new ArgumentOutOfRangeException("index");

            _buffer[index] = value;
        }
    }

    private char[] _buffer;
    private int _bufferPos;

    public MutString() : this(DEFAULT_CAPACITY) { }
    public MutString(MutString? other)
    {
        if (other is not null)
        {
            _buffer = _arrayPool.Rent(other.Capacity);
            this.Append(other);
        }
        else
        {
            _buffer = _arrayPool.Rent(DEFAULT_CAPACITY);
        }
    }
    public MutString(int initialCapacity)
    {
        int capacity = initialCapacity > 0 ? initialCapacity : DEFAULT_CAPACITY;
        _buffer = _arrayPool.Rent(capacity);
    }
    public MutString(string value)
    {
        if (value != null)
        {
            int capacity = value.Length > 0 ? value.Length : DEFAULT_CAPACITY;
            _buffer = _arrayPool.Rent(capacity);
            this.Append(value);
        }
        else
        {
            _buffer = _arrayPool.Rent(DEFAULT_CAPACITY);
        }
    }
    public MutString(char[] value) : this(value != null ? value.AsSpan() : Span<char>.Empty) { }
    public MutString(ReadOnlySpan<char> value)
    {
        if (!value.IsEmpty)
        {
            int capacity = value.Length > 0 ? value.Length : DEFAULT_CAPACITY;
            _buffer = _arrayPool.Rent(capacity);
            this.Append(value);
        }
        else
        {
            _buffer = _arrayPool.Rent(DEFAULT_CAPACITY);
        }
    }

    ///<summary>
    /// Appends a new line.
    ///</summary>
    public MutString AppendLine() => Append(Environment.NewLine);
    ///<summary>
    /// Appends a string and new line without memory allocation.
    ///</summary>
    public MutString AppendLine(string value)
    {
        Append(value);
        Append(Environment.NewLine);

        return this;
    }

    ///<summary>
    /// Appends a <see cref="MutString"/> without memory allocation.
    ///</summary>
    public MutString Append(MutString? value)
    {
        if (value is null)
            return this;

        int n = value.Length;
        if (n > 0)
        {
            EnsureCapacity(n);

            value.Span.TryCopyTo(new Span<char>(_buffer, _bufferPos, n));
            _bufferPos += n;
        }

        return this;
    }
    ///<summary>
    /// Allocates on the array's creation, and on boxing values.
    ///</summary>
    public MutString Append(params object[] values)
    {
        if (values != null)
        {
            int len = values.Length;
            for (var i = 0; i < len; i++)
                this.Append<object>(values[i]);
        }

        return this;
    }
    ///<summary>
    /// Appends values. Allocates when boxing.
    ///</summary>
    public MutString Append<T>(params T[] values)
    {
        if (values != null)
        {
            int len = values.Length;
            for (var i = 0; i < len; i++)
                Append(values[i]);
        }

        return this;
    }
    ///<summary>
    /// Appends <see cref="{T}"/>. Allocates when boxing.
    ///</summary>
    private MutString Append<T>(T value)
    {
        if (value is null)
            return this;

        switch (value)
        {
            case string: Append((string)(object)value); break;
            case char: Append((char)(object)value); break;
            case char[]: Append((char[])(object)value); break;
            case int: Append((int)(object)value); break;
            case long: Append((long)(object)value); break;
            case bool: Append((bool)(object)value); break;
            case DateTime: Append((DateTime)(object)value); break;
            case decimal: Append((decimal)(object)value); break;
            case float: Append((float)(object)value); break;
            case double: Append((double)(object)value); break;
            case byte: Append((byte)(object)value); break;
            case sbyte: Append((sbyte)(object)value); break;
            case ulong: Append((ulong)(object)value); break;
            case uint: Append((uint)(object)value); break;
            default: Append(value.ToString()); break;
        }

        return this;
    }
    ///<summary>
    /// Appends a <see cref="string"/> without memory allocation.
    ///</summary>
    public MutString Append(string? value)
    {
        if (value is null)
            return this;

        int n = value?.Length ?? 0;
        if (n > 0)
        {
            EnsureCapacity(n);

            value.AsSpan().TryCopyTo(new Span<char>(_buffer, _bufferPos, n));
            _bufferPos += n;
        }

        return this;
    }
    ///<summary> 
    /// Appends a <see cref="char"/> without memory allocation.
    ///</summary>
    public MutString Append(char value)
    {
        if (_bufferPos >= Capacity)
            EnsureCapacity(1);

        _buffer[_bufferPos++] = value;

        return this;
    }
    ///<summary>
    /// Appends a <see cref="bool"/> without memory allocation.
    ///</summary>
    public MutString Append(bool value)
    {
        if (value)
            Append(_bool[1]);
        else
            Append(_bool[0]);

        return this;
    }
    ///<summary>
    /// Appends a <see cref="char"/>[] without memory allocation.
    ///</summary>
    public MutString Append(char[] value)
    {
        if (value == null)
            return this;

        Append(value.AsSpan());

        return this;
    }
    ///<summary>
    /// Appends a <see cref="ReadOnlySpan{char}"/> without memory allocation.
    ///</summary> 
    public MutString Append(ReadOnlySpan<char> value)
    {
        int n = value.Length;
        if (n > 0)
        {
            EnsureCapacity(n);
            value.TryCopyTo(new Span<char>(_buffer, _bufferPos, n));
            _bufferPos += n;
        }

        return this;
    }
    ///<summary>
    /// Appends an <see cref="object.ToString()"/>. Allocates memory.
    ///</summary>
    public MutString Append(object? value)
    {
        if (value is null)
            return this;

        Append(value.ToString());

        return this;
    }
    ///<summary>
    /// Appends an <see cref="DateTime"/>. Allocates memory.
    ///</summary>
    public MutString Append(DateTime value) => Append(value, _defaultCulture);
    ///<summary>
    /// Appends an <see cref="DateTime"/>. Allocates memory.
    ///</summary>
    public MutString Append(DateTime value, CultureInfo culture) => Append(value.ToString(culture));
    ///<summary>
    /// Appends an <see cref="sbyte"/> without memory allocation.
    ///</summary>
    public MutString Append(sbyte value)
    {
        if (value < 0)
            Append((ulong)-((int)value), true);
        else
            Append((ulong)value, false);

        return this;
    }
    ///<summary>
    /// Appends an <see cref="byte"/> without memory allocation.
    ///</summary>
    public MutString Append(byte value) => Append(value, false);
    ///<summary>
    /// Appends an <see cref="uint"/> without memory allocation.
    ///</summary>
    public MutString Append(uint value) => Append((ulong)value, false);
    /// <summary>
    /// Appends a <see cref="ulong"/> without memory allocation.
    ///</summary>
    public MutString Append(ulong value) => Append(value, false);
    ///<summary>
    /// Appends an <see cref="short"/> without memory allocation.
    ///</summary>
    public MutString Append(short value) => Append((int)value);
    ///<summary>
    /// Appends an <see cref="int"/> without memory allocation.
    ///</summary>
    public MutString Append(int value)
    {
        bool isNegative = value < 0;
        if (isNegative)
            value = -value;

        Append((ulong)value, isNegative);

        return this;
    }
    ///<summary>
    /// Appends an <see cref="long"/> without memory allocation.
    ///</summary>
    public MutString Append(long value)
    {
        bool isNegative = value < 0;
        if (isNegative)
            value = -value;

        Append((ulong)value, isNegative);

        return this;
    }
    ///<summary>
    /// Appends a <see cref="float"/>. Allocates memory.
    ///</summary>
    public MutString Append(float value) => Append(value, _defaultCulture);
    ///<summary>
    /// Appends a <see cref="float"/>. Allocates memory.
    ///</summary>
    public MutString Append(float value, CultureInfo culture) => Append(value.ToString(culture));
    ///<summary>
    /// Appends a <see cref="decimal"/>. Allocates memory.
    ///</summary>
    public MutString Append(decimal value) => Append(value, _defaultCulture);
    ///<summary>
    /// Appends a <see cref="decimal"/>. Allocates memory.
    ///</summary>
    public MutString Append(decimal value, CultureInfo culture) => Append(value.ToString(culture));
    ///<summary>
    /// Appends a <see cref="double"/>. Allocates memory.
    ///</summary>
    public MutString Append(double value) => Append(value, _defaultCulture);
    ///<summary>
    /// Appends a <see cref="double"/>. Allocates memory.
    ///</summary>
    public MutString Append(double value, CultureInfo culture) => Append(value.ToString(culture));

    ///<summary>
    /// Clears values, and append new <see cref="MutString"/> without memory allocation.
    ///</summary>
    public MutString Set(MutString other)
    {
        Clear();
        Append(other);

        return this;
    }
    ///<summary>
    /// Clears values, and append new <see cref="string"/> without memory allocation.
    ///</summary>
    public MutString Set(string str)
    {
        Clear();
        Append(str);

        return this;
    }
    ///<summary>
    /// Clears values, and append new <see cref="char[]"/> without memory allocation.
    ///</summary>
    public MutString Set(char[] value) => Set(value.AsSpan());
    ///<summary>
    /// Clears values, and append new <see cref="ReadOnlySpan{char}"/> without memory allocation.
    ///</summary>
    public MutString Set(ReadOnlySpan<char> value)
    {
        Clear();
        Append(value);

        return this;
    }
    ///<summary>
    /// Clears values, and append new values. Will allocate a little memory due to boxing.
    ///</summary>
    public MutString Set(params object[] values)
    {
        Clear();

        for (int i = 0; i < values.Length; i++)
            Append(values[i]);

        return this;
    }

    ///<summary>
    /// Sets buffer pointer to zero.
    ///</summary>
    public void Clear() => _bufferPos = 0;

    ///<summary>
    /// Replaces all occurrences of a <see cref="string"/> by another one.
    ///</summary>
    public MutString Replace(string oldStr, string newStr)
    {
        if (_bufferPos == 0)
            return this;

        int oldstrLength = oldStr?.Length ?? 0;
        if (oldstrLength == 0)
            return this;

        if (newStr == null)
            newStr = "";

        int newStrLength = newStr.Length;

        int deltaLength = oldstrLength > newStrLength ? oldstrLength - newStrLength : newStrLength - oldstrLength;
        int size = ((_bufferPos / oldstrLength) * (oldstrLength + deltaLength)) + 1;
        int index = 0;
        char[] replacementChars = null!;
        int replaceIndex = 0;
        char firstChar = oldStr![0];

        // Create the new string into _replacement.
        for (int i = 0; i < _bufferPos; i++)
        {
            bool isToReplace = false;
            if (_buffer[i] == firstChar) // If first character found, check for the rest of the string to replace.
            {
                int k = 1; // Skip one char.
                while (k < oldstrLength && _buffer[i + k] == oldStr[k])
                    k++;

                isToReplace = (k == oldstrLength);
            }
            if (isToReplace) // Do the replacement.
            {
                if (replaceIndex == 0)
                {
                    // First replacement target.
                    replacementChars = _arrayPool.Rent(size);
                    // Copy first set of char that did not match.
                    new Span<char>(_buffer, 0, i).TryCopyTo(new Span<char>(replacementChars, 0, i));
                    index = i;
                }

                replaceIndex++;
                i += oldstrLength - 1;

                for (int k = 0; k < newStrLength; k++)
                    replacementChars[index++] = newStr[k];
            }
            else if (replaceIndex > 0) // No replacement, copy the old character.
                replacementChars[index++] = _buffer[i]; // Todo: Could batch these up instead one at a time!
        }

        if (replaceIndex > 0)
        {
            // Copy back the new string into _chars.
            EnsureCapacity(index - _bufferPos);

            new Span<char>(replacementChars, 0, index).TryCopyTo(new Span<char>(_buffer));

            _arrayPool.Return(replacementChars);
            _bufferPos = index;
        }

        return this;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 0;
            for (var i = 0; i < _bufferPos; i++)
                hash += _buffer[i].GetHashCode();

            return 31 * hash + _bufferPos;
        }
    }

    ///<summary>
    /// Allocates a string.
    ///</summary>
    public override string ToString()
    {
        if (_bufferPos == 0)
            return string.Empty;

        unsafe
        {
            fixed (char* sourcePtr = &_buffer[0])
                return new string(sourcePtr, 0, _bufferPos);
        }
    }

    public override bool Equals(object? obj) => obj is MutString ? Equals((MutString)obj) : false;
    public bool Equals(MutString? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (other.Length != this.Length)
            return false;

        for (var i = 0; i < _bufferPos; i++)
        {
            if (!this._buffer[i].Equals(other._buffer[i]))
                return false;
        }

        return true;
    }

    #region Implementations
    public IEnumerator<char> GetEnumerator()
    {
        for (int i = 0; i < _bufferPos; i++)
            yield return _buffer[i];
    }

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    int IComparable<MutString>.CompareTo(MutString? other) => this.CompareTo(other, StringComparison.Ordinal);
    int IComparable.CompareTo(object? obj)
    {
        if (obj is null)
            return -1;

        if (obj is MutString mut)
            return this.CompareTo(mut, StringComparison.Ordinal);

        throw new ArgumentException($"Object is not a {nameof(MutString)}.");
    }

#if !NETSTANDARD1_3
    object ICloneable.Clone() => this.Clone();
#endif
    #endregion

    #region Static
    /// <summary>
    /// Gets a new instance of <see cref="MutString"/>
    /// </summary>
    public static MutString Create(int initialCapacity = MutString.DEFAULT_CAPACITY) => new MutString(initialCapacity);
    /// <summary>
    /// Gets a new instance of <see cref="MutString"/>
    /// </summary>
    public static MutString Create(MutString other) => new MutString(other);
    /// <summary>
    /// Gets a new instance of <see cref="MutString"/>
    /// </summary>
    public static MutString Create(string other) => new MutString(other);
    /// <summary>
    /// Gets a new instance of <see cref="MutString"/>
    /// </summary>
    public static MutString Create(ReadOnlySpan<char> other) => new MutString(other);
    /// <summary>
    /// Gets a new instance of <see cref="MutString"/>
    /// </summary>
    public static MutString Create(char[] other) => new MutString(other);

    /// <summary>
    /// Clones existing <see cref="MutString"/> to a new instance with a new buffer.
    /// </summary>
    public static MutString Clone(MutString toClone) => toClone.Clone();

    public static int CompareTo(MutString span, MutString other, StringComparison comparisonType)
        => span.CompareTo(other, comparisonType);
    #endregion

    #region Operators
    public static implicit operator MutString(string value) => new MutString(value);
    public static implicit operator MutString(char[] value) => new MutString(value);
    public static implicit operator MutString(ReadOnlySpan<char> value) => new MutString(value);
    public static implicit operator MutString(Span<char> value) => new MutString(value);

    public static bool operator ==(MutString? lhs, MutString? rhs) => lhs?.Equals(rhs) ?? false;
    public static bool operator !=(MutString? lhs, MutString? rhs) => !(lhs == rhs);
    public static bool operator ==(MutString? lhs, ReadOnlySpan<char> rhs) => lhs is not null ? MemoryExtensions.Equals(lhs.Span, rhs, StringComparison.Ordinal) : false;
    public static bool operator !=(MutString? lhs, ReadOnlySpan<char> rhs) => !(lhs == rhs);
    public static bool operator ==(ReadOnlySpan<char> lhs, MutString? rhs) => rhs is not null ? MemoryExtensions.Equals(lhs, rhs.Span, StringComparison.Ordinal) : false;
    public static bool operator !=(ReadOnlySpan<char> lhs, MutString? rhs) => !(lhs == rhs);
    public static bool operator ==(MutString? lhs, Span<char> rhs) => lhs == (ReadOnlySpan<char>)rhs;
    public static bool operator !=(MutString? lhs, Span<char> rhs) => !(lhs == rhs);
    public static bool operator ==(Span<char> lhs, MutString? rhs) => (ReadOnlySpan<char>)lhs == rhs;
    public static bool operator !=(Span<char> lhs, MutString? rhs) => !(lhs == rhs);
    public static bool operator ==(MutString? lhs, string? rhs) => lhs == rhs.AsSpan();
    public static bool operator !=(MutString? lhs, string? rhs) => !(lhs == rhs);
    public static bool operator ==(string? lhs, MutString? rhs) => lhs.AsSpan() == rhs;
    public static bool operator !=(string? lhs, MutString? rhs) => !(lhs == rhs);
    #endregion

    #region Private
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private MutString Append(ulong value, bool isNegative)
    {
        // Allocate enough memory to handle any ulong number.
        int length = GetIntLength(value);


        EnsureCapacity(length + (isNegative ? 1 : 0));
        var buffer = _buffer;

        // Handle the negative case.
        if (isNegative)
        {
            buffer[_bufferPos++] = '-';
        }
        if (value <= 9)
        {
            //between 0-9.
            buffer[_bufferPos++] = _charNumbers[value];
            return this;
        }

        // Copy the digits with reverse in mind.
        _bufferPos += length;
        int nbChars = _bufferPos - 1;
        do
        {
            buffer[nbChars--] = _charNumbers[value % 10];
            value /= 10;
        } while (value != 0);

        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureCapacity(int appendLength)
    {
        int capacity = Capacity;
        int pos = _bufferPos;
        if (pos + appendLength > capacity)
        {
            capacity = capacity + appendLength + DEFAULT_CAPACITY - (capacity - pos);
            char[] newBuffer = _arrayPool.Rent(capacity);

            if (pos > 0)
                new Span<char>(_buffer, 0, _bufferPos).TryCopyTo(new Span<char>(newBuffer)); // Copy data.

            _arrayPool.Return(_buffer);

            _buffer = newBuffer;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetIntLength(ulong n)
    {
        if (n < 10L) return 1;
        if (n < 100L) return 2;
        if (n < 1000L) return 3;
        if (n < 10000L) return 4;
        if (n < 100000L) return 5;
        if (n < 1000000L) return 6;
        if (n < 10000000L) return 7;
        if (n < 100000000L) return 8;
        if (n < 1000000000L) return 9;
        if (n < 10000000000L) return 10;
        if (n < 100000000000L) return 11;
        if (n < 1000000000000L) return 12;
        if (n < 10000000000000L) return 13;
        if (n < 100000000000000L) return 14;
        if (n < 1000000000000000L) return 15;
        if (n < 10000000000000000L) return 16;
        if (n < 100000000000000000L) return 17;
        if (n < 1000000000000000000L) return 18;
        if (n < 10000000000000000000L) return 19;

        return 20;
    }
    #endregion
}
