using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeBox.Compilation
{
    internal static class TypeExtensions
    {
        public static bool IsEqualGenericType(this Type type1, Type type2)
        {
            return type1.IsGenericType && type2.IsGenericType &&
                   (type1.GetGenericTypeDefinition() == type2.GetGenericTypeDefinition());
        }

        public static Type GetFirstGenericArgument(this Type type)
        {
            return type.GetGenericArguments()[0];
        }

        public static Type GetFirstGenericArgument(this Type type, Type interfaceType)
        {
            if (type.IsEqualGenericType(interfaceType))
            {
                return type.GetGenericArguments()[0];
            }

            var iface = type.GetInterfaces().FirstOrDefault(x => x.IsGenericType && (x.GetGenericTypeDefinition() == interfaceType));
            return iface != null ? iface.GetGenericArguments()[0] : null;
        }

        public static bool ImplementsIEnumerable(this Type type)
        {
            return type.GetInterfaces().Any(x => x.IsGenericType && (x.GetGenericTypeDefinition() == typeof(IEnumerable<>)));
        }

        public static bool ImplementsGenericType(this Type type1, Type type2)
        {
            return type1.GetInterfaces().Any(x => x.IsGenericType && (x.GetGenericTypeDefinition() == type2));
        }

        public static bool ImplementsOrIsGenericType(this Type type1, Type type2)
        {
            return ImplementsGenericType(type1, type2) || IsEqualGenericType(type1, type2);
        }

        public static string GetFriendlyFullName(this Type type)
        {
            string friendlyName = type.Name;
            if (type.IsGenericType)
            {
                int iBacktick = friendlyName.IndexOf('`');
                if (iBacktick > 0)
                {
                    friendlyName = friendlyName.Remove(iBacktick);
                }
                friendlyName += "<";
                Type[] typeParameters = type.GetGenericArguments();
                for (int i = 0; i < typeParameters.Length; ++i)
                {
                    string typeParamName = typeParameters[i].Name;
                    friendlyName += (i == 0 ? typeParamName : "," + typeParamName);
                }
                friendlyName += ">";
            }

            return friendlyName;
        }

        public static string GetFriendlyName(this Type type)
        {
            string friendlyName = type.Name;
            if (type.IsGenericType)
            {
                int iBacktick = friendlyName.IndexOf('`');
                if (iBacktick > 0)
                {
                    friendlyName = friendlyName.Remove(iBacktick);
                }
            }

            return friendlyName;
        }
    }
}
