using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using TypeBox.Parser;

namespace TypeBox.Compilation
{
    internal partial class Visitor : TypeBoxBaseVisitor<Expression>
    {
        private IScope _scope;

        public Visitor(IScope scope)
        {
            
            _scope = scope;
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

        public override Expression VisitBlockItemList(TypeBoxParser.BlockItemListContext context)
        {
            // TODO: Improve!!
            if (context.blockItemList() != null)
            {
                return Expression.Block(new[] {Visit(context.blockItemList()), Visit(context.blockItem())});
            }

            return Visit(context.blockItem());
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
            return Visit(context.expressionStatement());
        }

        public override Expression VisitExpressionStatement(TypeBoxParser.ExpressionStatementContext context)
        {
            if (context.expression() != null)
            {
                return Visit(context.expression());
            }

            return Expression.Empty();
        }

        private Type GetTypeFromName(string typeName)
        {
            switch (typeName)
            {
                case "void":
                    return typeof (void);
                case "int":
                    return typeof(int);
                case "float":
                    return typeof(float);
            }

            return null;
        }

        public override Expression VisitInitializer(TypeBoxParser.InitializerContext context)
        {
            return Visit(context.assignmentExpression());
        }

        private Expression GetInitDeclarator(TypeBoxParser.InitDeclaratorContext context, Type type)
        {
            Expression varExpression = _scope.CreateLocalVariable(context.declarator().NAME().GetText(), type);

            if (context.initializer() != null)
            {
                return Expression.Assign(varExpression, Visit(context.initializer()));
            }

            return Expression.Empty();
        }

        private IList<Expression> GetInitDeclaratorList(TypeBoxParser.InitDeclaratorListContext context, Type type)
        {
            var assignmentList = context.initDeclaratorList() != null ? GetInitDeclaratorList(context.initDeclaratorList(), type) : new List<Expression>();

            assignmentList.Add(GetInitDeclarator(context.initDeclarator(), type));
            return assignmentList;
        }

        public override Expression VisitDeclaration(TypeBoxParser.DeclarationContext context)
        {
            string typeName = context.typeSpecifier().GetText();
            Type type = GetTypeFromName(typeName);

            if (type == null)
            {
                throw new NotSupportedException("Invalid type specifier");
            }

            if (context.initDeclaratorList() == null)
            {
                throw new SyntaxErrorException("You must specify a declarator along with the type");
            }

            return Expression.Block(GetInitDeclaratorList(context.initDeclaratorList(), type));
        }
    }
}
