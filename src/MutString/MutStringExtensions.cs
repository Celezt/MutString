using System;

namespace Celezt.String;

public static class MutStringExtensions
{
    /// <summary>
    /// Clones existing <see cref="MutString"/> to a new instance with a new buffer.
    /// </summary>
    public static MutString Clone(this MutString? toClone) => new MutString(toClone);

    public static int CompareTo(this MutString span, MutString? other, StringComparison comparisonType)
    {
        if (other is null)
            return -1;

        return span.ReadOnlySpan.CompareTo(other.ReadOnlySpan, comparisonType);
    }
}
