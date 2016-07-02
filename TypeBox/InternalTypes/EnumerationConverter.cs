using System;
using System.Collections.Generic;
using System.Linq;
using TypeBox.Compilation;

namespace TypeBox.InternalTypes
{
    static class EnumerationConverter
    {
        public static TDest[] ToArray<TDest, TSource>(IEnumerable<TSource> source)
        {
            // Very nice!
            return source.Select(x => (TDest)Convert.ChangeType(x, typeof(TDest))).ToArray();
        }

        public static IEnumerable<TDest> ToIEnumerable<TDest, TSource>(IEnumerable<TSource> source)
        {
            // Very nice!
            return source.Select(x => (TDest)Convert.ChangeType(x, typeof(TDest)));
        }

        public static IList<TDest> ToList<TDest, TSource>(IEnumerable<TSource> source)
        {
            // Very nice!
            return source.Select(x => (TDest)Convert.ChangeType(x, typeof(TDest))).ToList();
        }

        public static TypeBoxArray<TDest> ToTypeBoxArray<TDest, TSource>(IEnumerable<TSource> source)
        {
            // Very nice!
            return new TypeBoxArray<TDest>(source.Select(x => (TDest)Convert.ChangeType(x, typeof(TDest))));
        }

        public static bool IsAssignableEnumeration(Type type)
        {
            return type.IsArray || type.ImplementsOrIsGenericType(typeof(IEnumerable<>)) || type.ImplementsOrIsGenericType(typeof(IList<>)) || type.IsEqualGenericType(typeof(TypeBoxArray<>));
        }

        public static object AssignEnumeration<TAssign, TDest, TSource>(IEnumerable<TSource> source)
        {
            var assignType = typeof (TAssign);

            if (assignType.IsArray)
            {
                return ToArray<TDest, TSource>(source);
            }

            // Check for exact match (most specific to least specific):

            if (assignType.IsEqualGenericType(typeof(TypeBoxArray<>)))
            {
                return ToTypeBoxArray<TDest, TSource>(source);
            }

            if (assignType.IsEqualGenericType(typeof(IList<>)))
            {
                return ToList<TDest, TSource>(source);
            }

            if (assignType.IsEqualGenericType(typeof(IEnumerable<>)))
            {
                return ToIEnumerable<TDest, TSource>(source);
            }

            // Check for interface implementation (most specific to least specific):

            if (assignType.ImplementsOrIsGenericType(typeof(IList<>)))
            {
                return ToList<TDest, TSource>(source);
            }

            if (assignType.ImplementsOrIsGenericType(typeof(IEnumerable<>)))
            {
                return ToIEnumerable<TDest, TSource>(source);
            }

            

            throw new NotImplementedException($"Enumeration convertion of type '{assignType.Name}' is not supported");
        }

        public static Type GetElementType(Type enumerationType)
        {
            if (enumerationType.IsArray)
            {
                return enumerationType.GetElementType();
            }

            if (enumerationType.IsEqualGenericType(typeof(TypeBoxArray<>)))
            {
                return enumerationType.GetFirstGenericArgument();
            }

            if (enumerationType.ImplementsOrIsGenericType(typeof(IList<>)))
            {
                return enumerationType.GetFirstGenericArgument(typeof(IList<>));
            }

            if (enumerationType.ImplementsOrIsGenericType(typeof(IEnumerable<>)))
            {
                return enumerationType.GetFirstGenericArgument(typeof(IEnumerable<>));
            }

            throw new NotImplementedException($"Get Element Type of type '{enumerationType.Name}' is not supported");
        }
    }
}
