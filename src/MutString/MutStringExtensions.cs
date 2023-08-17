﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celezt.String;

public static class MutStringExtensions
{
    /// <summary>
    /// Clones existing <see cref="MutString"/> to a new instance with a new buffer.
    /// </summary>
    public static MutString Clone(this MutString toClone) => new MutString(toClone);

    public static int CompareTo(this MutString span, MutString other, StringComparison comparisonType)
        => span.ReadOnlySpan.CompareTo(other.ReadOnlySpan, comparisonType);
}