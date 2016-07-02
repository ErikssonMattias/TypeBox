using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeBox.Compilation
{
    internal static class Extensions
    {
        public static Type GetItemType<T>(this IEnumerable<T> enumerable)
        {
            return typeof(T);
        }
    }
}
