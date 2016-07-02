using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace TypeBox.Compilation
{
    internal class EnvironmentScope : IScope
    {
        private readonly List<ParameterExpression> _envs = new List<ParameterExpression>();
        private readonly List<Type> _types = new List<Type>();

        public EnvironmentScope()
        {
        }

        public void AddEnvironmentParameter(ParameterExpression param)
        {
            _envs.Add(param);
        }

        public void AddType(Type type)
        {
            _types.Add(type);
        }

        public void AddTypes(IEnumerable<Type> types)
        {
            _types.AddRange(types);
        }

        public ParameterExpression[] GetParameterExpressionArray()
        {
            return _envs.ToArray();
        }

        public Expression Set(string key, Expression exp)
        {
            var env = _envs.FirstOrDefault(x => x.Type.GetMember(key).Length > 0);

            if (env == null)
            {
                return null;
            }

            return Expression.PropertyOrField(env, key);
            //throw new NotImplementedException();
            //return Expression.Call(_environmentParameter, LuTable.SetMethodInfo, Expression.Constant(key), Expression.Convert(exp, typeof(object)));
        }

        public Expression Get(string key)
        {
            // TODO: might want to improve this, to search for all possible matches with the same name over several environments
            var env = _envs.FirstOrDefault(x => x.Type.GetMember(key).Length > 0);

            if (env == null)
            {
                //return null;
                // TODO: fix exception classes
                throw new TypeBoxParseException(new Exception($"Can't find the variable '{key}'"));
            }
            
            if ((env.Type.GetProperty(key) != null) || (env.Type.GetField(key) != null))
            {
                return Expression.PropertyOrField(env, key);
            }

            var eventInfo = env.Type.GetEvent(key);
            if (eventInfo != null)
            {
                return new EventInfoExpression(eventInfo, env);
            }

            var methodInfos = env.Type.GetMethods().Where(x => x.Name == key).ToArray();
            if (methodInfos.Length > 0)
            {
                return new MethodInfosExpression(methodInfos.Select(x => new MethodInfosExpression.MethodInfoInstancePair { Instance = env, MethodInfo = x }));
            }

            return null;
            //throw new NotImplementedException();
        }

        public MethodInfo[] GetMethodInfos(string key, out ParameterExpression instance)
        {
            // TODO: might want to improve this, to search for all possible matches with the same name over several environments
            var env = _envs.FirstOrDefault(x => x.Type.GetMethod(key) != null);

            if (env == null)
            {
                //return null;
                // TODO: fix exception classes
                throw new TypeBoxParseException(new Exception($"Can't find the variable '{key}'"));
            }

            instance = env;

            return env.Type.GetMethods().Where(x => x.Name == key).ToArray();
        }

        public IEnumerable<MemberInfo> GetMemberInfos(string key, out ParameterExpression instance)
        {
            // TODO: might want to improve this, to search for all possible matches with the same name over several environments
            var env = _envs.FirstOrDefault(x => x.Type.GetMember(key).Length > 0);

            if (env == null)
            {
                //return null;
                // TODO: fix exception classes
                throw new TypeBoxParseException(new Exception($"Can't find the variable '{key}'"));
            }

            instance = env;

            return env.Type.GetMembers().Where(x => x.Name == key);
        }

        public Type GetType(string key)
        {
            return _types.FirstOrDefault(x => x.GetFriendlyName() == key);
        }

        //public MethodInfo GetFunction(string key, IEnumerable<Expression> argumentList, out ParameterExpression instanceExpression)
        //{
        //    var typeArray = argumentList.Select(ae => ae.Type).ToArray();
        //    var env = _envs.FirstOrDefault(x => x.Type.GetMethod(key, typeArray) != null);

        //    instanceExpression = env;

        //    if (env == null)
        //    {
        //        //return null;
        //        throw new TypeBoxParseException(new Exception($"Can't find the function '{key}'"));
        //    }

        //    return env.Type.GetMethod(key, typeArray);
        //}

        public IScope Parent => null;

        public IEnumerable<ParameterExpression> LocalVariables { get { return null; } }

        public ParameterExpression CreateLocalVariable(string name)
        {
            throw new NotImplementedException();
        }

        public ParameterExpression CreateLocalVariable(string name, Type type)
        {
            throw new NotImplementedException();
        }
    }
}