using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Celezt.Text;

internal static class SpanHelpers
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CopyTo(ReadOnlySpan<char> span, Span<char> destination,
        ReadOnlySpan<char> oldValue, ReadOnlySpan<char> newValue, ReadOnlySpan<int> indices)
    {
        int currentIndex = 0;
        int destinationIndex = 0;

        for (int i = 0; i < indices.Length; i++)
        {
            int replacementIndex = indices[i];

            // Copy over the non-matching portion of the original that precedes this occurrence of oldValue.
            int count = replacementIndex - currentIndex;

            if (count != 0)
            {
                span.Slice(currentIndex, count).CopyTo(destination.Slice(destinationIndex));
                destinationIndex += count;
            }
            currentIndex = replacementIndex + oldValue.Length;

            // Copy over newValue to replace the oldValue.
            newValue.CopyTo(destination.Slice(destinationIndex));
            destinationIndex += newValue.Length;
        }

        // Copy over the final non-matching portion at the end of the string.
        span.Slice(currentIndex).CopyTo(destination.Slice(destinationIndex));
        return destinationIndex + span.Length - currentIndex;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int IndexOfChar(ref char searchSpace, char chr, int length)
    {
        for (int index = 0; index < length; index++)
            if (Unsafe.Add(ref searchSpace, index) == chr)
                return index;

        return -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int IndexOf(ref char searchSpace, int searchSpaceLength, ref char value, int valueLength)
    {
        Debug.Assert(searchSpaceLength >= 0);
        Debug.Assert(valueLength >= 0);

        nint offset = 0;
        char valueHead = value;
        int valueTailLength = valueLength - 1;
        int searchSpaceMinusValueTailLength = searchSpaceLength - valueTailLength;

        ref byte valueTail = ref Unsafe.As<char, byte>(ref Unsafe.Add(ref value, 1));
        int remainingSearchSpaceLength = searchSpaceMinusValueTailLength;

        while (remainingSearchSpaceLength > 0)
        {
            // Do a quick search for the first element of "value".
            int relativeIndex = IndexOfChar(ref Unsafe.Add(ref searchSpace, offset), valueHead, remainingSearchSpaceLength);
            if (relativeIndex < 0)
                break;

            remainingSearchSpaceLength -= relativeIndex;
            offset += relativeIndex;

            if (remainingSearchSpaceLength <= 0)
                break;  // The unsearched portion is now shorter than the sequence we're looking for. So it can't be there.

            // Found the first element of "value". See if the tail matches.
            if (SequenceEqual(
                    ref Unsafe.As<char, byte>(ref Unsafe.Add(ref searchSpace, offset + 1)),
                    ref valueTail,
                    (nuint)(uint)valueTailLength * 2))
            {
                return (int)offset;  // The tail matched. Return a successful find.
            }

            remainingSearchSpaceLength--;
            offset++;
        }

        return -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool SequenceEqual(ref byte first, ref byte second, nuint length)
    {
        bool result;

        // On 32-bit, this will always be true since sizeof(nuint) == 4.
        if (length < sizeof(uint))
        {
            uint differentBits = 0;
            nuint offset = (length & 2);
            if (offset != 0)
            {
                differentBits = Unsafe.ReadUnaligned<ushort>(ref first);
                differentBits -= Unsafe.ReadUnaligned<ushort>(ref second);
            }
            if ((length & 1) != 0)
            {
                differentBits |= (uint)Unsafe.AddByteOffset(ref first, (IntPtr)(void*)offset) -
                                (uint)Unsafe.AddByteOffset(ref second, (IntPtr)(void*)offset);
            }
            result = (differentBits == 0);
        }
        else
        {
            nuint offset = length - sizeof(uint);
            uint differentBits = Unsafe.ReadUnaligned<uint>(ref first) - Unsafe.ReadUnaligned<uint>(ref second);
            differentBits |= Unsafe.ReadUnaligned<uint>(ref Unsafe.AddByteOffset(ref first, (IntPtr)(void*)offset)) -
                            Unsafe.ReadUnaligned<uint>(ref Unsafe.AddByteOffset(ref second, (IntPtr)(void*)offset));
            result = (differentBits == 0);
        }

        return result;
    }
}
