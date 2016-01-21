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

        public EnvironmentScope()
        {
        }

        public void AddEnvironmentParameter(ParameterExpression param)
        {
            _envs.Add(param);
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
            var env = _envs.FirstOrDefault(x => x.Type.GetMember(key).Length > 0);

            if (env == null)
            {
                //return null;
                // TODO: fix exception classes
                throw new TypeBoxParseException(new Exception("Can't find the variable"));
            }

            return Expression.PropertyOrField(env, key);
            //throw new NotImplementedException();
        }

        public MethodInfo GetFunction(string key, IEnumerable<Expression> argumentList, out ParameterExpression instanceExpression)
        {
            var typeArray = argumentList.Select(ae => ae.Type).ToArray();
            var env = _envs.FirstOrDefault(x => x.Type.GetMethod(key, typeArray) != null);

            instanceExpression = env;

            if (env == null)
            {
                //return null;
                throw new TypeBoxParseException(new Exception("Can't find the function"));
            }

            return env.Type.GetMethod(key, typeArray);
        }

        public IScope Parent { get { return null; } }

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