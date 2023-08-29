using System;

namespace Celezt.Text;

public static class MutStringExtensions
{
    /// <summary>
    /// Clones existing <see cref="MutString"/> to a new instance.
    /// </summary>
    public static MutString Clone(this MutString? toClone) => new MutString(toClone);

    /// <summary>
    /// Compares the specified <paramref name="mutString"/> and <paramref name="other"/> using the specified <paramref name="comparisonType"/>,
    /// and returns an integer that indicates their relative position in the sort order.
    /// </summary>
    /// <param name="mutString">The source <see cref="MutString"/>.</param>
    /// <param name="other">The value to compare with the source <see cref="MutString"/>.</param>
    /// <param name="comparisonType">One of the enumeration values that determines how the <paramref name="mutString"/> and <paramref name="other"/> are compared.</param>
    public static int CompareTo(this MutString mutString, MutString? other, StringComparison comparisonType)
    {
        if (other is null)
            return -1;

        return mutString.ReadOnlySpan.CompareTo(other.ReadOnlySpan, comparisonType);
    }
    /// <summary>
    /// Compares the specified <paramref name="mutString"/> and <paramref name="other"/> using the specified <paramref name="comparisonType"/>,
    /// and returns an integer that indicates their relative position in the sort order.
    /// </summary>
    /// <param name="mutString">The source <see cref="MutString"/>.</param>
    /// <param name="other">The value to compare with the source <see cref="MutString"/>.</param>
    /// <param name="comparisonType">One of the enumeration values that determines how the <paramref name="mutString"/> and <paramref name="other"/> are compared.</param>
    public static int CompareTo(this MutString mutString, ReadOnlySpan<char> other, StringComparison comparisonType)
        => mutString.ReadOnlySpan.CompareTo(other, comparisonType);

    /// <summary>
    /// Copies data into destination without memory allocation.
    /// </summary>
    /// /// <param name="mutString">The source <see cref="MutString"/>.</param>
    /// <param name="destination">The <see cref="MutString"/> to copy items into.</param>
    public static void CopyTo(this MutString mutString, MutString destination) => destination.Set(mutString);
    /// <summary>
    /// Copies data into destination without memory allocation.
    /// </summary>
    /// /// <param name="mutString">The source <see cref="MutString"/>.</param>
    /// <param name="destination">The <see cref="char"/>[] to copy items into.</param>
    public static void CopyTo(this MutString mutString, char[] destination) => mutString.CopyTo(destination.AsSpan());
    /// <summary>
    /// Copies data into destination without memory allocation.
    /// </summary>
    /// /// <param name="mutString">The source <see cref="MutString"/>.</param>
    /// <param name="destination">The <see cref="Span{Char}"/> to copy items into.</param>
    public static void CopyTo(this MutString mutString, Span<char> destination) => mutString.Span.CopyTo(destination);
}
