using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Celezt.String;

public partial class MutString
{
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

            value.Span.TryCopyTo(_buffer.AsSpan(_bufferPosition, n));
            _bufferPosition += n;
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
            case string: Append(Unsafe.As<string>(value)); break;
            case char[]: Append(Unsafe.As<char[]>(value)); break;
            case char: Append((char)(object)value); break;
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

            value.AsSpan().CopyTo(_buffer.AsSpan(_bufferPosition, n));
            _bufferPosition += n;
        }

        return this;
    }
    ///<summary> 
    /// Appends a <see cref="char"/> without memory allocation.
    ///</summary>
    public MutString Append(char value)
    {
        if (_bufferPosition >= Capacity)
            EnsureCapacity(1);

        _buffer[_bufferPosition++] = value;

        return this;
    }
    ///<summary>
    /// Appends a <see cref="bool"/> without memory allocation.
    ///</summary>
    public unsafe MutString AppendOld(bool value)
    {
#if NET5_0_OR_GREATER
        ref char bufferRef = ref MemoryMarshal.GetArrayDataReference(_buffer);
        int position = _bufferPosition;

        if (value)
        {
            EnsureCapacity(4);
            Unsafe.Add(ref bufferRef, position++) = 'T';
            Unsafe.Add(ref bufferRef, position++) = 'r';
            Unsafe.Add(ref bufferRef, position++) = 'u';
            Unsafe.Add(ref bufferRef, position++) = 'e';
        }
        else
        {
            EnsureCapacity(5);
            Unsafe.Add(ref bufferRef, position++) = 'F';
            Unsafe.Add(ref bufferRef, position++) = 'a';
            Unsafe.Add(ref bufferRef, position++) = 'l';
            Unsafe.Add(ref bufferRef, position++) = 's';
            Unsafe.Add(ref bufferRef, position++) = 'e';
        }

        _bufferPosition = position;
#else
        if (value)
            Append("True");
        else
            Append("False");
#endif
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
            value.TryCopyTo(_buffer.AsSpan(_bufferPosition, n));
            _bufferPosition += n;
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

    /// <summary>
    /// Removes everything after start index without memory allocation. 
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public MutString Remove(int startIndex)
    {
        if (startIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(startIndex));

        int oldLength = _bufferPosition;

        if (startIndex >= oldLength)
            throw new ArgumentOutOfRangeException(nameof(oldLength));

        _bufferPosition = startIndex + 1;

        return this;
    }
    /// <summary>
    /// Removes content between start index and length without memory allocation. 
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public MutString Remove(int startIndex, int length)
    {
        if (startIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(startIndex));
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length));

        int oldLength = _bufferPosition;

        if (length > oldLength - startIndex)
            throw new ArgumentOutOfRangeException(nameof(length));

        if (length == 0)
            return this;
         
        int newLength = startIndex + length;

        if (newLength == oldLength)
        {
            _bufferPosition -= length;
            return this;
        }

        _buffer.AsSpan(startIndex + length).CopyTo(_buffer.AsSpan(startIndex));
        _bufferPosition -= length;

        return this;
    }

    ///<summary>
    /// Replaces all occurrences of a <see cref="char"/> by another one.
    ///</summary>
    public MutString Replace(char oldChar, char newChar)
    {
        if (oldChar == newChar)
            return this;

        Span<char> bufferSpan = this.Span;
        ref char bufferRef = ref MemoryMarshal.GetReference(bufferSpan);

        int firstIndex = SpanHelpers.IndexOfChar(ref bufferRef, oldChar, bufferSpan.Length);

        if (firstIndex < 0)
            return this;

        int remainingLength = _bufferPosition - firstIndex;
        int length = firstIndex;

        // Copy the remaining characters, doing the replacement as we go.
        ref ushort remainingRef = ref Unsafe.Add(ref Unsafe.As<char, ushort>(ref bufferRef), length);

        for (int i = 0; i < remainingLength; ++i)
        {
            ushort currentChar = Unsafe.Add(ref remainingRef, i);
            Unsafe.Add(ref remainingRef, i) = currentChar == oldChar ? newChar : currentChar;
        }

        return this;
    }
    ///<summary>
    /// Replaces all occurrences of a <see cref="string"/> by another one.
    ///</summary>
    ///<exception cref="ArgumentNullException"></exception>
    public MutString Replace(string oldValue, string? newValue)
    {
        if (string.IsNullOrEmpty(oldValue))
            throw new ArgumentNullException(nameof(oldValue));

        newValue ??= string.Empty;

        Replace(oldValue.AsSpan(), newValue.AsSpan());

        return this;
    }
    ///<summary>
    /// Replaces all occurrences of a <see cref="ReadOnlySpan{char}"/> by another one.
    ///</summary>
    public MutString Replace(ReadOnlySpan<char> oldValue, ReadOnlySpan<char> newValue)
    {
        if (_bufferPosition == 0)
            return this;

        if (oldValue.Length == 0)
            return this;

        if (oldValue.Length == 1 && newValue.Length == 1)   // If both old and new is just one character.
            return Replace(oldValue[0], newValue[0]);

        // Only allocate what can possible be fit.
        Span<int> replacementIndices = stackalloc int[_bufferPosition / (newValue.Length == 0 ? 1 : newValue.Length)];
        Span<char> bufferSpan = _buffer.AsSpan();
        ref char bufferRef = ref MemoryMarshal.GetReference(bufferSpan);
        char firstChar = oldValue[0];
        int indicesLength = 0;
        int index = 0;

        if (oldValue.Length == 1)
        {
            while (true)
            {
                int position = SpanHelpers.IndexOfChar(ref Unsafe.Add(ref bufferRef, index), firstChar, bufferSpan.Length - index);

                if (position < 0)
                    break;

                replacementIndices[indicesLength++] = index + position;
                index += position + 1;
            }
        }
        else
        {
            ref char oldRef = ref MemoryMarshal.GetReference(oldValue);
            while (true)
            {
                int pos = SpanHelpers.IndexOf(ref Unsafe.Add(ref bufferRef, index), Length - index, ref oldRef, oldValue.Length);
                if (pos < 0)
                {
                    break;
                }
                replacementIndices[indicesLength++] = index + pos;
                index += pos + oldValue.Length;
            }
        }

        if (indicesLength == 0) // If no oldValues where found.
            return this;

        replacementIndices = replacementIndices.Slice(0, indicesLength);
        int newLength = _bufferPosition + (newValue.Length - oldValue.Length) * indicesLength;

        if (newLength > int.MaxValue)
            throw new OutOfMemoryException();

        int capacity = Capacity;
        if (newLength > capacity)   // Needs to allocated more space.
        {
            capacity += newLength - capacity + DEFAULT_CAPACITY - (capacity - _bufferPosition);

            char[] newBuffer = _arrayPool.Rent(capacity);   // Rent a bigger array.
            Span<char> newBufferSpan = newBuffer.AsSpan();

            int length = SpanHelpers.CopyTo(bufferSpan, newBufferSpan, oldValue, newValue, replacementIndices);

            _arrayPool.Return(_buffer);
            _buffer = newBuffer;
            _bufferPosition = length;
        }
        else
        {
            Span<char> tempSpan = stackalloc char[_bufferPosition];
            bufferSpan.Slice(0, _bufferPosition).CopyTo(tempSpan);

            int length = SpanHelpers.CopyTo(tempSpan, bufferSpan, oldValue, newValue, replacementIndices);

            _bufferPosition = length;
        }

        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private MutString Append(ulong value, bool isNegative)
    {
        // Allocate enough memory to handle any ulong number.
        int length = GetIntLength(value);
        EnsureCapacity(length + (isNegative ? 1 : 0));

#if NET5_0_OR_GREATER
        ref char bufferRef =  ref MemoryMarshal.GetArrayDataReference(_buffer);

        // Handle the negative case.
        if (isNegative)
            Unsafe.Add(ref bufferRef, _bufferPosition++) = '-';

        if (value <= 9)
        {
            //between 0-9.
            Unsafe.Add(ref bufferRef, _bufferPosition++) = (char)('0' + value);

            return this;
        }
        else
        {
            // Copy the digits with reverse in mind.
            _bufferPosition += length;
            int numberCharacters = _bufferPosition - 1;
            do
            {
                Unsafe.Add(ref bufferRef, numberCharacters--) = (char)('0' + value % 10);
                value /= 10;
            } while (value != 0);
        }
#else
        Span<char> span = _buffer;

        // Handle the negative case.
        if (isNegative)
            span[_bufferPosition++] = '-';

        if (value <= 9)
        {
            //between 0-9.
            span[_bufferPosition++] = (char)('0' + value);
        }
        else
        {
            // Copy the digits with reverse in mind.
            _bufferPosition += length;
            int numberCharacters = _bufferPosition - 1;
            do
            {
                span[numberCharacters--] = (char)('0' + value % 10);
                value /= 10;
            } while (value != 0);
        }
#endif

        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureCapacity(int appendedLength)
    {
        int capacity = Capacity;
        if (_bufferPosition + appendedLength > capacity)
        {
            capacity += appendedLength + DEFAULT_CAPACITY - (capacity - _bufferPosition);
            char[] newBuffer = _arrayPool.Rent(capacity);

            if (_bufferPosition > 0)
                _buffer.AsSpan(0, _bufferPosition).CopyTo(newBuffer); // Copy data.

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
}