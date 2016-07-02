using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace TypeBox.Compilation
{
    internal abstract class TypeBoxExpression : Expression
    {
        
    }

    internal class MethodInfosExpression : TypeBoxExpression
    {
        public IEnumerable<MethodInfoInstancePair> MethodInfos { get; private set; }

        public class MethodInfoInstancePair
        {
            public MethodInfo MethodInfo;
            public Expression Instance;
        }

        public MethodInfosExpression(IEnumerable<MethodInfoInstancePair> methodInfos)
        {
            MethodInfos = methodInfos;
        }
    }

    internal class MemberInfoExpression : TypeBoxExpression
    {
        public MemberInfo MemberInfo { get; private set; }
        public Expression Instance { get; private set; }

        public MemberInfoExpression(MemberInfo memberInfo, Expression instance)
        {
            MemberInfo = memberInfo;
            Instance = instance;
        }
    }

    internal class EventInfoExpression : MemberInfoExpression
    {
        public EventInfo EventInfo { get; private set; }

        public EventInfoExpression(EventInfo eventInfo, Expression instance) : base(eventInfo, instance)
        {
            EventInfo = eventInfo;
        }
    }

    internal interface IScope
    {
        Expression Set(string key, Expression exp);
        Expression Get(string key);
        MethodInfo[] GetMethodInfos(string key, out ParameterExpression instance);
        IEnumerable<MemberInfo> GetMemberInfos(string key, out ParameterExpression instance);
        Type GetType(string key);
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

        public MethodInfo[] GetMethodInfos(string key, out ParameterExpression instance)
        {
            return _parentScope.GetMethodInfos(key, out instance);
        }

        public IEnumerable<MemberInfo> GetMemberInfos(string key, out ParameterExpression instance)
        {
            return _parentScope.GetMemberInfos(key, out instance);
        }

        public Type GetType(string key)
        {
            // We do not support local type declaration at the moment, walk up to the environment scope
            return _parentScope.GetType(key);
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
