﻿using System.Globalization;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Celezt.Text;

///<summary>
/// Mutable <see cref="string"/>.
///</summary>
#if !NETSTANDARD1_3
[Serializable]
#endif
public partial class MutString : IComparable, IComparable<MutString>, IEnumerable, IEnumerable<char>, IEquatable<MutString>
#if !NETSTANDARD1_3
    , ICloneable
#endif
{
    private const int DEFAULT_CAPACITY = 16;

    private readonly static CultureInfo _defaultCulture = CultureInfo.InvariantCulture;
    private readonly static ArrayPool<char> _arrayPool = ArrayPool<char>.Shared;

    public int Length
    {
        get => _bufferPosition;
        set
        {
            if (value > Capacity || value < 0)
                throw new ArgumentOutOfRangeException("value");

            _bufferPosition = value;
        }
    }

    public int Capacity => _buffer.Length;

    public bool IsEmpty => _bufferPosition == 0;

    public Memory<char> Memory => _buffer.AsMemory(0, _bufferPosition);
    public Span<char> Span => _buffer.AsSpan(0, _bufferPosition);
    public ReadOnlySpan<char> ReadOnlySpan => _buffer.AsSpan(0, _bufferPosition);

    public char this[int index]
    {
        get
        {
            if (index > _bufferPosition || index < 0)
                throw new IndexOutOfRangeException();

            return _buffer[index];
        }
        set
        {
            if (index > _bufferPosition || index < 0)
                throw new ArgumentOutOfRangeException("index");

            _buffer[index] = value;
        }
    }

    private char[] _buffer;
    private int _bufferPosition;

    public MutString() : this(DEFAULT_CAPACITY) { }
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

    ///<summary>
    /// Sets buffer pointer to zero.
    ///</summary>
    public void Clear() => _bufferPosition = 0;

    /// <summary>
    /// Returns a value indicating whether a specified character occurs.
    /// </summary>
    /// <returns>If it exist.</returns>
    public bool Contains(char value) => this.IndexOf(value) >= 0;
    /// <summary>
    /// Returns a value indicating whether a specified character occurs.
    /// </summary>
    /// <returns>If it exist.</returns>
    public bool Contains(string value) => this.Contains(value.AsSpan());
    /// <summary>
    /// Returns a value indicating whether a specified character occurs.
    /// </summary>
    /// <returns>If it exist.</returns>
    public bool Contains(ReadOnlySpan<char> value) => this.IndexOf(value) >= 0;

    public int IndexOf(char value) => this.IndexOf(value, 0);
    public int IndexOf(char value, int startIndex) => this.IndexOf(value, startIndex, _bufferPosition - startIndex);
    public int IndexOf(char value, int startIndex, int count)
    {
        if (startIndex < 0 || startIndex >= _bufferPosition)
            throw new ArgumentOutOfRangeException(nameof(startIndex));

        if (count < 0 || count > _bufferPosition - startIndex)
            throw new ArgumentOutOfRangeException(nameof(count));

#if NET5_0_OR_GREATER
    ref char bufferRef = ref MemoryMarshal.GetArrayDataReference(_buffer);
#else
        ref char bufferRef = ref MemoryMarshal.GetReference(_buffer.AsSpan());
#endif
        int index = SpanHelpers.IndexOfChar(ref Unsafe.Add(ref bufferRef, startIndex), value, count);

        return index < 0 ? index : index + startIndex;
    }
    public int IndexOf(string value) => this.IndexOf(value, 0);
    public int IndexOf(string value, int startIndex) => this.IndexOf(value, startIndex, _bufferPosition - startIndex);
    public int IndexOf(string value, int startIndex, int count) => this.IndexOf(value.AsSpan(), startIndex, count);
    public int IndexOf(ReadOnlySpan<char> value) => this.IndexOf(value, 0);
    public int IndexOf(ReadOnlySpan<char> value, int startIndex) => this.IndexOf(value, startIndex, _bufferPosition - startIndex);
    public int IndexOf(ReadOnlySpan<char> value, int startIndex, int count)
    {
        if (startIndex < 0 || startIndex >= _bufferPosition)
            throw new ArgumentOutOfRangeException(nameof(startIndex));

        if (count < 0 || count > _bufferPosition - startIndex)
            throw new ArgumentOutOfRangeException(nameof(count));

#if NET5_0_OR_GREATER
        ref char bufferRef = ref MemoryMarshal.GetArrayDataReference(_buffer);
#else
        ref char bufferRef = ref MemoryMarshal.GetReference(_buffer.AsSpan());
#endif
        ref char valueRef = ref MemoryMarshal.GetReference(value);

        int index = SpanHelpers.IndexOf(ref Unsafe.Add(ref bufferRef, startIndex), count, ref valueRef, value.Length);

        return index < 0 ? index : index + startIndex;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 0;
            for (var i = 0; i < _bufferPosition; i++)
                hash += _buffer[i].GetHashCode();

            return 31 * hash + _bufferPosition;
        }
    }

    ///<summary>
    /// Allocates a string.
    ///</summary>
    public override string ToString()
    {
        if (_bufferPosition == 0)
            return string.Empty;

        unsafe
        {
            fixed (char* sourcePtr = &_buffer[0])
                return new string(sourcePtr, 0, _bufferPosition);
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

        for (var i = 0; i < _bufferPosition; i++)
        {
            if (!this._buffer[i].Equals(other._buffer[i]))
                return false;
        }

        return true;
    }

    public IEnumerator<char> GetEnumerator()
    {
        for (int i = 0; i < _bufferPosition; i++)
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

    /// <summary>
    /// Compares the specified <paramref name="mutString"/> and <paramref name="other"/> using the specified <paramref name="comparisonType"/>,
    /// and returns an integer that indicates their relative position in the sort order.
    /// </summary>
    /// <param name="mutString">The source <see cref="MutString"/>.</param>
    /// <param name="other">The value to compare with the source <see cref="MutString"/>.</param>
    /// <param name="comparisonType">One of the enumeration values that determines how the <paramref name="mutString"/> and <paramref name="other"/> are compared.</param>
    public static int CompareTo(MutString mutString, MutString? other, StringComparison comparisonType)
        => mutString.CompareTo(other, comparisonType);
    /// <summary>
    /// Compares the specified <paramref name="mutString"/> and <paramref name="other"/> using the specified <paramref name="comparisonType"/>,
    /// and returns an integer that indicates their relative position in the sort order.
    /// </summary>
    /// <param name="mutString">The source <see cref="MutString"/>.</param>
    /// <param name="other">The value to compare with the source <see cref="MutString"/>.</param>
    /// <param name="comparisonType">One of the enumeration values that determines how the <paramref name="mutString"/> and <paramref name="other"/> are compared.</param>
    public static int CompareTo(MutString mutString, ReadOnlySpan<char> other, StringComparison comparisonType)
    => mutString.CompareTo(other, comparisonType);

    public static explicit operator MutString(string value) => new MutString(value);
    public static explicit operator MutString(char[] value) => new MutString(value);
    public static explicit operator MutString(ReadOnlySpan<char> value) => new MutString(value);
    public static explicit operator MutString(Span<char> value) => new MutString(value);

    public static implicit operator ReadOnlySpan<char>(MutString value) => value.ReadOnlySpan;
    public static implicit operator Span<char>(MutString value) => value.Span;

    public static explicit operator string(MutString value) => value.ToString();
    public static explicit operator char[](MutString value)
    {
#if NET5_0_OR_GREATER
        char[] array = GC.AllocateUninitializedArray<char>(value.Length, true);
#else
        char[] array = new char[value.Length];
#endif
        value.Span.CopyTo(array);

        return array;
    }

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
}
