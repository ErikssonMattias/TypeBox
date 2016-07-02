using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Antlr4.Runtime;
using TypeBox.Compilation;
using TypeBox.Parser;

namespace TypeBox
{
    public class TypeBoxScriptEngine
    {
        //private readonly ParameterExpression _environmentParameter = Expression.Variable(typeof(T));

        public TypeBoxScriptEngine()
        {
        }

        public T Evaluate<T>(string expression)
        {
            return CompileExpression<T>(expression)();
        }

        public T Evaluate<T, TEnv>(TEnv env, string expression)
        {
            return CompileExpression<T, TEnv>(expression)(env);
        }

        public Func<T> CompileExpression<T>(string expression)
        {
            return () => CompileExpressionInternal<T, object>(expression)(new object());
        }

        public Func<TEnv, T> CompileExpression<T, TEnv>(string expression)
        {
            return CompileExpressionInternal<T, TEnv>(expression);
        }

        private Func<TEnv, T> CompileExpressionInternal<T, TEnv>(string expression)
        {
            var lexer = new TypeBoxLexer(new AntlrInputStream(expression));
            var parser = new TypeBoxParser(new CommonTokenStream(lexer));

            var globalScope = new EnvironmentScope();
            globalScope.AddEnvironmentParameter(Expression.Parameter(typeof(TEnv)));

            var visitor = new Visitor(globalScope);

            var parserErrorListener = new MemoryParserErrorListener();
            parser.RemoveErrorListeners();
            lexer.RemoveErrorListeners();
            parser.RemoveParseListeners();

            parser.AddErrorListener(parserErrorListener);

            try
            {
                Expression body = Expression.Convert(visitor.VisitExpression(parser.expression()), typeof(object));

                if (parserErrorListener.Messages.Any())
                {
                    throw new TypeBoxParseException(parserErrorListener.Messages);
                }

                var innerLambda = (Func<TEnv, object>)Expression.Lambda(body, globalScope.GetParameterExpressionArray()).Compile();
                return environment => (T)Convert.ChangeType(innerLambda(environment), typeof(T));
            }
            catch (Exception e)
            {
                if (parserErrorListener.Messages.Any())
                {
                    throw new TypeBoxParseException(parserErrorListener.Messages);
                }

                throw new TypeBoxParseException(e);
            }
        }

        private Expression CompileInternal(EnvironmentScope scope, string code)
        {
            var lexer = new TypeBoxLexer(new AntlrInputStream(code));
            var parser = new TypeBoxParser(new CommonTokenStream(lexer));

            var visitor = new Visitor(scope);

            var parserErrorListener = new MemoryParserErrorListener();
            parser.AddErrorListener(parserErrorListener);

            try
            {
                Expression content = visitor.Visit(parser.compileUnit());
                if (parserErrorListener.Messages.Any())
                {
                    throw new TypeBoxParseException(parserErrorListener.Messages);
                }

                return content;
            }
            catch (Exception e)
            {
                if (parserErrorListener.Messages.Any())
                {
                    throw new TypeBoxParseException(parserErrorListener.Messages);
                }

                throw new TypeBoxParseException(e);
            }
        }

        public Action Compile(IEnumerable<Type> types, string code)
        {
            var globalScope = new EnvironmentScope();

            if (types != null)
            {
                globalScope.AddTypes(types);
            }

            return Expression.Lambda<Action>(CompileInternal(globalScope, code), globalScope.GetParameterExpressionArray()).Compile();
        }

        public Action<TEnv1> Compile<TEnv1>(IEnumerable<Type> types, string code)
        {
            var globalScope = new EnvironmentScope();
            globalScope.AddEnvironmentParameter(Expression.Parameter(typeof(TEnv1)));

            if (types != null)
            {
                globalScope.AddTypes(types);
            }

            return Expression.Lambda<Action<TEnv1>>(CompileInternal(globalScope, code), globalScope.GetParameterExpressionArray()).Compile();
        }

        public Action<TEnv1, TEnv2> Compile<TEnv1, TEnv2>(IEnumerable<Type> types, string code)
        {
            var globalScope = new EnvironmentScope();
            globalScope.AddEnvironmentParameter(Expression.Parameter(typeof(TEnv1)));
            globalScope.AddEnvironmentParameter(Expression.Parameter(typeof(TEnv2)));

            if (types != null)
            {
                globalScope.AddTypes(types);
            }

            return Expression.Lambda<Action<TEnv1, TEnv2>>(CompileInternal(globalScope, code), globalScope.GetParameterExpressionArray()).Compile();
        }

        public Action<TEnv1, TEnv2, TEnv3> Compile<TEnv1, TEnv2, TEnv3>(IEnumerable<Type> types, string code)
        {
            var globalScope = new EnvironmentScope();
            globalScope.AddEnvironmentParameter(Expression.Parameter(typeof(TEnv1)));
            globalScope.AddEnvironmentParameter(Expression.Parameter(typeof(TEnv2)));
            globalScope.AddEnvironmentParameter(Expression.Parameter(typeof(TEnv3)));

            if (types != null)
            {
                globalScope.AddTypes(types);
            }

            return Expression.Lambda<Action<TEnv1, TEnv2, TEnv3>>(CompileInternal(globalScope, code), globalScope.GetParameterExpressionArray()).Compile();
        }

        public Action<TEnv1> Compile<TEnv1>(string code)
        {
            return Compile<TEnv1>(null, code);
        }

        public Action<TEnv1, TEnv2> Compile<TEnv1, TEnv2>(string code)
        {
            return Compile<TEnv1, TEnv2>(null, code);
        }

        public Action<TEnv1, TEnv2, TEnv3> Compile<TEnv1, TEnv2, TEnv3>(string code)
        {
            return Compile<TEnv1, TEnv2, TEnv3>(null, code);
        }

        private class MemoryParserErrorListener : IAntlrErrorListener<IToken>
        {
            private readonly List<TypeBoxCompileMessage> _messages = new List<TypeBoxCompileMessage>();

            public void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg,
                RecognitionException e)
            {
                _messages.Add(new TypeBoxCompileMessage(line, charPositionInLine, msg));
            }

            public IEnumerable<TypeBoxCompileMessage> Messages
            {
                get { return _messages; }
            }
        }

        public void Execute(string code)
        {
            Compile(null, code)();
        }

        public void Execute<TEnv1>(string code, TEnv1 environment)
        {
            Compile<TEnv1>(code)(environment);
        }
    }
}
