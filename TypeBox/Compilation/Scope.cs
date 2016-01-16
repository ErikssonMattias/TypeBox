using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace TypeBox.Compilation
{
    internal interface IScope
    {
        Expression Set(string key, Expression exp);
        Expression Get(string key);
        MethodInfo GetFunction(string key, IEnumerable<Expression> argumentList, out ParameterExpression instanceExpression);
        IScope Parent { get; }
        IEnumerable<ParameterExpression> LocalVariables { get; }
        ParameterExpression CreateLocalVariable(string name, Type type);
    }

    internal class Scope : IScope
    {
        private readonly IScope _parentScope;
        private readonly Dictionary<string, ParameterExpression> _localsByName = new Dictionary<string, ParameterExpression>();
        private readonly List<ParameterExpression> _localVariables = new List<ParameterExpression>();

        public Scope(IScope parentScope)
        {
            _parentScope = parentScope;
        }

        public IScope Parent
        {
            get { return _parentScope; }
        }

        public IEnumerable<ParameterExpression> LocalVariables
        {
            get { return _localVariables; }
        }

        public Expression Set(string key, Expression exp)
        {
            if (_localsByName.ContainsKey(key))
            {
                return Expression.Assign(_localsByName[key], Expression.Convert(exp, typeof(object)));
            }

            return _parentScope.Set(key, exp);
        }

        public Expression Get(string key)
        {
            if (_localsByName.ContainsKey(key))
            {
                return _localsByName[key];
            }

            return _parentScope.Get(key);
        }

        public MethodInfo GetFunction(string key, IEnumerable<Expression> argumentList, out ParameterExpression instanceExpression)
        {
            return _parentScope.GetFunction(key, argumentList, out instanceExpression);
        }

        public ParameterExpression CreateLocalVariable(string name, Type type)
        {
            var parameterExpression = Expression.Variable(type, name);
            _localVariables.Add(parameterExpression);
            _localsByName[name] = parameterExpression;
            return parameterExpression;
        }
    }
}
