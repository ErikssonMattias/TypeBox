using System;

namespace TypeBox.Compilation
{
    public class TypeBoxSettings
    {
        public bool CSharpTypes = true;
        public bool TypeScriptTypes = true;
        public Type NumberType = typeof(double);

        public bool NullSafeMemberAccess = false;
        public bool NullSafeMemberAccessOperator = true;

        public bool NullSafeFunctionCalls = true;
    }
}