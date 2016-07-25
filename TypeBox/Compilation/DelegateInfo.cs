using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeBox.Compilation
{
    internal class DelegateInfo
    {
        public readonly Type ReturnType;
        public readonly Type[] ParameterTypes;

        public DelegateInfo(Type returnType, IEnumerable<Type> parameterTypes)
        {
            ReturnType = returnType;
            ParameterTypes = parameterTypes.ToArray();
        }
    }
}
