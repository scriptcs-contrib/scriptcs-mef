using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace ScriptCs.ComponentModel.Composition
{
    internal static class Extensions
    {
        internal static ReadOnlyCollection<T> ToReadOnlyCollection<T>(this IEnumerable<T> collection)
        {
            return new ReadOnlyCollectionBuilder<T>(collection).ToReadOnlyCollection();
        }
    }
}
