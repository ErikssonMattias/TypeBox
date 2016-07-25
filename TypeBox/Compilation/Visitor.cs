using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using TypeBox.InternalTypes;
using TypeBox.Parser;

namespace TypeBox.Compilation
{
    internal partial class Visitor : TypeBoxBaseVisitor<Expression>
    {
        private IScope _scope;
        private readonly TypeBoxSettings _settings = new TypeBoxSettings();
        private readonly Stack<LabelTarget> _breakStack = new Stack<LabelTarget>();
        private readonly Stack<LabelTarget> _continueStack = new Stack<LabelTarget>();
        private readonly Stack<LabelTarget> _returnStack = new Stack<LabelTarget>();
        private readonly Stack<Type> _expectedTypeStack = new Stack<Type>();

        public Visitor(IScope scope)
        {
            _scope = scope;
        }

        public Visitor(IScope scope, TypeBoxSettings settings)
        {
            _scope = scope;
            _settings = settings;
        }

        public override Expression VisitCompileUnit(TypeBoxParser.CompileUnitContext context)
        {
            var expressions = new List<Expression>();
            foreach (var child in context.children)
            {
                var expression = Visit(child);
                if (expression != null)
                {
                    expressions.Add(expression);
                }
            }

            return Expression.Block(expressions);
        }

        public override Expression VisitBlock(TypeBoxParser.BlockContext context)
        {
            if (context.ChildCount == 0)
            {
                return Expression.Empty();
            }

            _scope = new Scope(_scope);
            var stats = context.children.Select(Visit);
            var blockExpression = Expression.Block(_scope.LocalVariables, stats);
            _scope = _scope.Parent;
            return blockExpression;
        }

        private IList<Expression> GetBlockItemList(TypeBoxParser.BlockItemListContext context)
        {
            var blockItemList = context.blockItemList() != null ? GetBlockItemList(context.blockItemList()) : new List<Expression>();

            blockItemList.Add(VisitBlockItem((context.blockItem())));
            
            return blockItemList;
        }

        public override Expression VisitBlockItemList(TypeBoxParser.BlockItemListContext context)
        {
            _scope = new Scope(_scope);

            var blockItems = GetBlockItemList(context);

            if (blockItems.Count == 0)
            {
                blockItems.Add(Expression.Empty());
            }
            
            var blockExpression = Expression.Block(_scope.LocalVariables, blockItems);
            _scope = _scope.Parent;
            return blockExpression;
        }

        public override Expression VisitBlockItem(TypeBoxParser.BlockItemContext context)
        {
            if (context.declaration() != null)
            {
                return Visit(context.declaration());
            }

            return Visit(context.statement());
        }

        public override Expression VisitStatement(TypeBoxParser.StatementContext context)
        {
            if (context.expressionStatement() != null)
            {
                return Visit(context.expressionStatement());
            }

            if (context.compoundStatement() != null)
            {
                return Visit(context.compoundStatement());
            }

            if (context.iterationStatement() != null)
            {
                return Visit(context.iterationStatement());
            }

            if (context.jumpStatement() != null)
            {
                return VisitJumpStatement(context.jumpStatement());
            }

            return Visit(context.selectionStatement());
        }

        public override Expression VisitCompoundStatement(TypeBoxParser.CompoundStatementContext context)
        {
            if (context.blockItemList() != null)
            {
                return Visit(context.blockItemList());
            }

            return Expression.Empty();
        }

        public override Expression VisitExpressionStatement(TypeBoxParser.ExpressionStatementContext context)
        {
            if (context.expression() != null)
            {
                return Visit(context.expression());
            }

            return Expression.Empty();
        }

        public override Expression VisitSelectionStatement(TypeBoxParser.SelectionStatementContext context)
        {
            TypeBoxParser.StatementContext elseStatement = context.statement(1);
            if (elseStatement != null)
            {
                return Expression.IfThenElse(Visit(context.expression()), Visit(context.statement(0)), Visit(elseStatement));
            }

            return Expression.IfThen(Visit(context.expression()), Visit(context.statement(0)));
        }

        public override Expression VisitForStatement(TypeBoxParser.ForStatementContext context)
        {
            _scope = new Scope(_scope);
            LabelTarget breakLabel = Expression.Label();
            LabelTarget continueLabel = Expression.Label();

            _breakStack.Push(breakLabel);
            _continueStack.Push(continueLabel);

            Expression varDeclaration = context.variableDeclaration() != null ? VisitVariableDeclaration(context.variableDeclaration()) : Expression.Empty();

            Expression conditionalExp = context.expression() != null ? VisitExpression(context.expression()) : Expression.Constant(true);
            Expression incrementExp = context.expression2() != null ? VisitExpression2(context.expression2()) : Expression.Empty();


            Expression innerBlock = VisitStatement(context.statement());

            BlockExpression forBlock = Expression.Block(
                _scope.LocalVariables,
                varDeclaration,
                Expression.Loop(
                    Expression.IfThenElse(
                        conditionalExp,
                        Expression.Block(
                            innerBlock,
                            Expression.Label(continueLabel),
                            incrementExp
                        ),
                        Expression.Break(breakLabel)
                    ),
                    breakLabel));


            _continueStack.Pop();
            _breakStack.Pop();
            _scope = _scope.Parent;
            return forBlock;
        }

        public override Expression VisitForOfStatement(TypeBoxParser.ForOfStatementContext context)
        {
            // Help from http://stackoverflow.com/questions/27175558/foreach-loop-using-expression-trees

            _scope = new Scope(_scope);

            Expression collection = VisitAssignmentExpression(context.assignmentExpression());

            // Currently only iteration over types that implement IEnumerable is possible, should be enough for 99% of the cases
            Type collectionType = collection.Type;
            Type elementType;
            /////////

            if (collectionType.IsInterface && collectionType.IsGenericType &&
                collectionType.GetGenericTypeDefinition() == typeof (IEnumerable<>))
            {
                elementType = collectionType.GetGenericArguments()[0];
            }
            else
            {
                var interfaces = collection.Type.GetInterfaces();
                var t =
                    interfaces.FirstOrDefault(
                        x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof (IEnumerable<>));

                if (t == null)
                {
                    throw new TypeBoxCompileException("Cannot loop over that type");
                }

                elementType = t.GetGenericArguments()[0];
            }

            var enumerableType = typeof(IEnumerable<>).MakeGenericType(elementType);
            var enumeratorType = typeof(IEnumerator<>).MakeGenericType(elementType);

            var enumeratorVar = Expression.Variable(enumeratorType, "enumerator");
            var getEnumeratorCall = Expression.Call(collection, enumerableType.GetMethod("GetEnumerator"));
            var enumeratorAssign = Expression.Assign(enumeratorVar, getEnumeratorCall);

            // The MoveNext method's actually on IEnumerator, not IEnumerator<T>
            var moveNextCall = Expression.Call(enumeratorVar, typeof(IEnumerator).GetMethod("MoveNext"));

            var breakLabel = Expression.Label();
            var continueLabel = Expression.Label();

            _breakStack.Push(breakLabel);
            _continueStack.Push(continueLabel);

            ParameterExpression loopVar = VisitSingleVariableDeclaration(context.singleVariableDeclaration(), elementType);

            Expression loopContent = VisitStatement(context.statement());

            var loop = Expression.Block(new[] { enumeratorVar },
                enumeratorAssign,
                Expression.Loop(
                    Expression.IfThenElse(
                        Expression.Equal(moveNextCall, Expression.Constant(true)),
                        Expression.Block(_scope.LocalVariables,
                            Expression.Assign(loopVar, Expression.Property(enumeratorVar, "Current")),
                            loopContent
                        ),
                        Expression.Break(breakLabel)
                    ),
                breakLabel,
                continueLabel)
            );

            /////////

            _continueStack.Pop();
            _breakStack.Pop();
            _scope = _scope.Parent;
            return loop;
        }

        public override Expression VisitWhileStatement(TypeBoxParser.WhileStatementContext context)
        {
            _scope = new Scope(_scope);
            LabelTarget breakLabel = Expression.Label();
            LabelTarget continueLabel = Expression.Label();

            _breakStack.Push(breakLabel);
            _continueStack.Push(continueLabel);

            Expression conditionalExp = VisitExpression(context.expression());

            Expression innerBlock = VisitStatement(context.statement());

            Expression whileBlock = 
                Expression.Loop(
                    Expression.IfThenElse(
                        conditionalExp,
                        innerBlock,
                        Expression.Break(breakLabel)
                    ),
                    breakLabel,
                    continueLabel
                );


            _continueStack.Pop();
            _breakStack.Pop();
            _scope = _scope.Parent;
            return whileBlock;
        }

        public override Expression VisitDoWhileStatement(TypeBoxParser.DoWhileStatementContext context)
        {
            _scope = new Scope(_scope);
            LabelTarget breakLabel = Expression.Label();
            LabelTarget continueLabel = Expression.Label();

            _breakStack.Push(breakLabel);
            _continueStack.Push(continueLabel);

            Expression conditionalExp = VisitExpression(context.expression());

            Expression innerBlock = VisitStatement(context.statement());

            Expression doWhileBlock =
                Expression.Loop(
                    Expression.Block(
                        innerBlock,
                        Expression.IfThenElse(
                            conditionalExp,
                            Expression.Continue(continueLabel),
                            Expression.Break(breakLabel)
                        )
                    ),
                    breakLabel,
                    continueLabel
                );


            _continueStack.Pop();
            _breakStack.Pop();
            _scope = _scope.Parent;
            return doWhileBlock;
        }

        public override Expression VisitJumpStatement(TypeBoxParser.JumpStatementContext context)
        {
            string statement = context.GetChild(0).GetText();

            switch (statement)
            {
                case "break":
                    return Expression.Break(_breakStack.Peek());

                case "continue":
                    return Expression.Continue(_continueStack.Peek());

                case "return":
                    if (context.assignmentExpression() != null)
                    {
                        return Expression.Return(_returnStack.Peek(), VisitCastAssignmentExpression(_returnStack.Peek().Type, context.assignmentExpression()));
                    }

                    return Expression.Return(_returnStack.Peek());
            }
            
            throw new NotImplementedException("The jump statement is not implemented");
        }

        private Type GetTypeFromName(string typeName)
        {
            if (_settings.CSharpTypes)
            {
                switch (typeName)
                {
                    case "char":
                        return typeof(char);
                    case "ushort":
                        return typeof(ushort);
                    case "short":
                        return typeof(short);
                    case "uint":
                        return typeof(uint);
                    case "int":
                        return typeof(int);
                    case "ulong":
                        return typeof(ulong);
                    case "long":
                        return typeof(long);
                    case "float":
                        return typeof(float);
                    case "double":
                        return typeof(double);
                    case "bool":
                        return typeof(bool);
                    case "string":
                        return typeof(string);
                }
            }

            if (_settings.TypeScriptTypes)
            {
                switch (typeName)
                {
                    case "void":
                        return typeof (void);
                    case "boolean":
                        return typeof (bool);
                    case "number":
                        return _settings.NumberType;
                    case "Array":
                        return typeof (TypeBoxArray<>);
                    case "string":
                        return typeof(string);
                }
            }

            // Common
            switch (typeName)
            {
                case "void":
                    return typeof(void);
            }

            // Test
            switch (typeName)
            {
                case "object":
                    return typeof(object);
            }

            return _scope.GetType(typeName);
        }

        private Type GetTypeFromTypeSpecifier(TypeBoxParser.TypeSpecifierContext context)
        {
            var typeName = context.basicType().GetText();
            Type basicType = GetTypeFromName(typeName);

            if (basicType == null)
            {
                throw new TypeBoxCompileException($"Could not find specified type '{typeName}'");
            }

            if (context.arraySpecifier() != null)
            {
                return typeof(TypeBoxArray<>).MakeGenericType(basicType);
            }

            if (context.typeSpecifierList() != null)
            {
                var genericParams = GetTypeListFromTypeSpecifierList(context.typeSpecifierList());

                if (!basicType.IsGenericType)
                {
                    throw new TypeBoxCompileException($"'{typeName}' is not a ganeric type!");
                }

                return basicType.MakeGenericType(genericParams.ToArray());
            }

            return basicType;
        }

        private IList<Type> GetTypeListFromTypeSpecifierList(TypeBoxParser.TypeSpecifierListContext context)
        {
            var typeList = context.typeSpecifierList() != null ? GetTypeListFromTypeSpecifierList(context.typeSpecifierList()) : new List<Type>();

            typeList.Add(GetTypeFromTypeSpecifier(context.typeSpecifier()));

            return typeList;
        }

        public override Expression VisitInitializer(TypeBoxParser.InitializerContext context)
        {
            return Visit(context.assignmentExpression());
        }

        //public override Expression VisitInitDeclarator(TypeBoxParser.InitDeclaratorContext context)
        //{
        //    Expression initializerExpression;
        //    ParameterExpression varExpression;
        //    GetInitDeclarator(context, out varExpression, out initializerExpression);

        //    if (initializerExpression != null)
        //    {
        //        return Expression.Assign(varExpression, CastAssignment(varExpression.Type, initializerExpression));
        //    }

        //    return Expression.Empty();
        //}

        public void GetInitDeclarator(TypeBoxParser.InitDeclaratorContext context, out ParameterSpacification specification)
        {
            specification = new ParameterSpacification {Name = context.declarator().NAME().GetText()};
            
            if (context.typeSpecifier() != null)
            {
                specification.Type = GetTypeFromTypeSpecifier(context.typeSpecifier());

                if (context.initializer() != null)
                {
                    specification.InitializerExpression = VisitCastAssignmentExpression(specification.Type, context.initializer().assignmentExpression());
                }

                return;
            }

            // If we don't have a type specified we must have an initializer
            if (context.initializer() != null)
            {
                specification.InitializerExpression = VisitCastAssignmentExpression(null, context.initializer().assignmentExpression());
                specification.Type = specification.InitializerExpression.Type;
            }
        }

        public class ParameterSpacification
        {
            public string Name;
            public Type Type;
            public Expression InitializerExpression;
        }

        private IList<ParameterSpacification> GetInitDeclaratorList(TypeBoxParser.InitDeclaratorListContext context)
        {
            var declaratorList = context.initDeclaratorList() != null ? GetInitDeclaratorList(context.initDeclaratorList()) : new List<ParameterSpacification>();

            ParameterSpacification parameterSpacification;
            GetInitDeclarator(context.initDeclarator(), out parameterSpacification);
            declaratorList.Add(parameterSpacification);
            return declaratorList;
        }

        public override Expression VisitFunctionDeclaration(TypeBoxParser.FunctionDeclarationContext context)
        {
            var lambdaName = context.NAME().GetText();

            var lambda = GetLambdaExpression(null, context.compoundStatement(), context.typeSpecifier(), context.initDeclaratorList(), lambdaName);

            var variable = _scope.CreateLocalVariable(lambdaName, lambda.Type);

            return Expression.Assign(variable, lambda);
        }

        private LambdaExpression GetLambdaExpression(Type expectedType, TypeBoxParser.CompoundStatementContext compoundStatementContext, TypeBoxParser.TypeSpecifierContext typeSpecifierContext, TypeBoxParser.InitDeclaratorListContext initDeclaratorListContext, string lambdaName)
        {
            // TODO: Take care of default values in parameter list
            IList<ParameterSpacification> parameterSpacifications;
            if (initDeclaratorListContext != null)
            {
                parameterSpacifications = GetInitDeclaratorList(initDeclaratorListContext);
            }
            else
            {
                parameterSpacifications = new List<ParameterSpacification>();
            }

            Type returnType = null;

            if (typeSpecifierContext != null)
            {
                returnType = GetTypeFromTypeSpecifier(typeSpecifierContext);
            }

            if (expectedType != null)
            {
                // Will must try to match expected type and declared type

                if (!expectedType.IsDelegate())
                {
                    throw new TypeBoxCompileException("Trying to declare a lambda that is expected to be of non-delegate type");
                }

                DelegateInfo delegateInfo = expectedType.GetDelegate();

                if (returnType == null)
                {
                    returnType = delegateInfo.ReturnType;
                }
                else
                {
                    if (delegateInfo.ReturnType != returnType)
                    {
                        throw new TypeBoxCompileException("Cannot match lambda expression return type");
                    }
                }

                if (parameterSpacifications.Count != delegateInfo.ParameterTypes.Length)
                {
                    throw new TypeBoxCompileException("Cannot match lambda expression: parameter count differs");
                }

                for (int i = 0; i < parameterSpacifications.Count; i++)
                {
                    if ((parameterSpacifications[i].Type != null) &&
                        (parameterSpacifications[i].Type != delegateInfo.ParameterTypes[i]))
                    {
                        throw new TypeBoxCompileException($"Cannot match lambda expression: parameter {i} type differs");
                    }

                    // If type is not specified for some/all parameters we derive it from expected type
                    parameterSpacifications[i].Type = delegateInfo.ParameterTypes[i];
                }
            }
            
            // We default to void if no type is specified.
            if (returnType == null)
            {
                returnType = typeof (void);
            }

            if (parameterSpacifications.Any(x => x.Type == null))
            {
                throw new TypeBoxCompileException("Function/lambda parameter types must be specified either explicitly or implicitly");
            }

            List<Type> delegateTypes = new List<Type>(parameterSpacifications.Select(x => x.Type));
            // The last type argument determines the return type of the delegate.
            delegateTypes.Add(returnType);
            Type delegateType = Expression.GetDelegateType(delegateTypes.ToArray());
            
            var returnLabel = Expression.Label(returnType);

            _returnStack.Push(returnLabel);

            _scope = new Scope(_scope);

            var parameterExpressions = parameterSpacifications.Select(x => _scope.CreateLocalVariable(x.Name, x.Type)).ToArray();

            var expressions = Visit(compoundStatementContext);
            LabelExpression labelExpression;
            if (returnType == typeof (void))
            {
                labelExpression = Expression.Label(returnLabel);
            }
            else
            {
                labelExpression = Expression.Label(returnLabel, Expression.Default(returnType));
            }

            var lambda = Expression.Lambda(
                delegateType,
                Expression.Block(
                    expressions,
                    labelExpression),
                lambdaName,
                parameterExpressions);

            _scope = _scope.Parent;
            
            _returnStack.Pop();
            
            return lambda;
        }

        public override Expression VisitVariableDeclaration(TypeBoxParser.VariableDeclarationContext context)
        {
            // variable declaration and initialization
            var parameterSpacifications = GetInitDeclaratorList(context.initDeclaratorList());

            List<Expression> initializeExpressions = new List<Expression>();
            foreach (var parameterSpacification in parameterSpacifications)
            {
                if (parameterSpacification.Type == null)
                {
                    throw new TypeBoxCompileException("Must specify type for variable");
                }

                var variableExpression = _scope.CreateLocalVariable(parameterSpacification.Name, parameterSpacification.Type);

                if (parameterSpacification.InitializerExpression != null)
                {
                    initializeExpressions.Add(Expression.Assign(variableExpression, parameterSpacification.InitializerExpression));
                }
            }

            if (initializeExpressions.Count > 0)
            {
                return Expression.Block(initializeExpressions);
            }

            //Expression[] initializationStatements = declarations.
            //    Where(x => x.InitializerExpression != null).
            //    Select(x => Expression.Assign(x.ParameterExpression, CastAssignment(x.ParameterExpression.Type, x.InitializerExpression))).ToArray();

            //if (initializationStatements.Length > 0)
            //{
            //    return Expression.Block(initializationStatements);
            //}

            return Expression.Empty();
        }

        public override Expression VisitDeclaration(TypeBoxParser.DeclarationContext context)
        {
            if (context.functionDeclaration() != null)
            {
                return Visit(context.functionDeclaration());
            }

            if (context.variableDeclaration() != null)
            {
                return Visit(context.variableDeclaration());
            }

            return Expression.Empty();
        }

        public ParameterExpression VisitSingleVariableDeclaration(TypeBoxParser.SingleVariableDeclarationContext context, Type type)
        {
            if (context.typeSpecifier() != null)
            {
                Type declareType = GetTypeFromTypeSpecifier(context.typeSpecifier());

                // TODO: Implement casting to desired type (if possible)
                if (declareType != type)
                {
                    throw new TypeBoxCompileException("Variable type specified must match collection");
                }
            }
            ParameterExpression varExpression = _scope.CreateLocalVariable(context.declarator().NAME().GetText(), type);
            return varExpression;
        }
    }
}
