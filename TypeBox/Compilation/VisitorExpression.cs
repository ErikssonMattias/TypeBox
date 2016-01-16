using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Antlr4.Runtime.Tree;
using TypeBox.Parser;

namespace TypeBox.Compilation
{
    internal partial class Visitor
    {
        public override Expression VisitPrimaryExpression(TypeBoxParser.PrimaryExpressionContext context)
        {
            if (context.constant() != null)
            {
                return Visit(context.constant());
            }


            return _scope.Get(context.NAME().GetText());
        }

        public override Expression VisitPostfixExpression(TypeBoxParser.PostfixExpressionContext context)
        {
            return ProcessPostFixExpression(context, false, null);
        }

        private Expression ProcessPostFixExpression(TypeBoxParser.PostfixExpressionContext context, bool findFunction, IEnumerable<Expression> functionArgumentList)
        {
            if (context.postfixExpression() != null)
            {
                IParseTree oper = context.GetChild(1);

                if (oper.GetText() == ".")
                {
                    if (findFunction)
                    {
                        Expression instance = Visit(context.postfixExpression());
                        var argumentList = functionArgumentList as Expression[] ?? functionArgumentList.ToArray();
                        var typeArray = argumentList.Select(x => x.Type).ToArray();

                        var functionName = context.NAME().GetText();
                        var func = instance.Type.GetMethod(functionName, typeArray);

                        if (func == null)
                        {
                            throw new LucCompileException(
                                string.Format("Could not found a matching function with name '{0}' and {1} arguments", functionName, argumentList.Length));
                        }

                        return Expression.Call(instance, func, argumentList);
                    }

                    return Expression.PropertyOrField(Visit(context.postfixExpression()), context.NAME().GetText());
                }

                switch (oper.GetText())
                {
                    case "(":
                        IEnumerable<Expression> argumentList = GetArgumentExpressionList(context.argumentExpressionList());
                        return ProcessPostFixExpression(context.postfixExpression(), true, argumentList);
                    case "++":
                        return Expression.PostIncrementAssign(Visit(context.postfixExpression()));
                    case "--":
                        return Expression.PostDecrementAssign(Visit(context.postfixExpression()));
                }

                throw new NotSupportedException("Not a supported postfix expression yet");
            }

            if (findFunction)
            {
                if (context.primaryExpression().NAME() == null)
                {
                    throw new NotSupportedException("Trying to make a function call to unknown identifier");
                }

                ParameterExpression instance;
                var argumentList = functionArgumentList as Expression[] ?? functionArgumentList.ToArray();
                var functionName = context.primaryExpression().NAME().GetText();
                var func = _scope.GetFunction(functionName, argumentList, out instance);

                if (func == null)
                {
                    throw new LucCompileException(
                        string.Format("Could not found a matching function with name '{0}' and {1} arguments", functionName, argumentList.Length));
                }

                return Expression.Call(instance, func, argumentList);
            }

            return Visit(context.primaryExpression());
        }

        public IList<Expression> GetArgumentExpressionList(TypeBoxParser.ArgumentExpressionListContext context)
        {
            if (context == null)
            {
                return new List<Expression>();
            }

            var argumentList = context.argumentExpressionList() != null ? GetArgumentExpressionList(context.argumentExpressionList()) : new List<Expression>();

            argumentList.Add(Visit(context.assignmentExpression()));
            return argumentList;
        }

        public override Expression VisitUnaryExpression(TypeBoxParser.UnaryExpressionContext context)
        {
            if (context.unaryOperator() != null)
            {
                IParseTree oper = context.GetChild(0);

                switch (oper.GetText())
                {
                    case "-":
                        return Expression.Negate(Visit(context.castExpression()));
                    case "+":
                        return Expression.UnaryPlus(Visit(context.castExpression()));
                    case "~":
                        return Expression.Not(Visit(context.castExpression()));
                    case "!":
                        return Expression.Not(Visit(context.castExpression()));
                }

                throw new NotSupportedException("Not a supported unary operator");
            }

            if (context.unaryExpression() != null)
            {
                IParseTree oper = context.GetChild(0);

                switch (oper.GetText())
                {
                    case "++":
                        return Expression.PreIncrementAssign(Visit(context.castExpression()));
                    case "--":
                        return Expression.PreDecrementAssign(Visit(context.castExpression()));
                }

                throw new NotSupportedException("Not a supported unary operator");
            }

            return Visit(context.postfixExpression());
        }

        public override Expression VisitCastExpression(TypeBoxParser.CastExpressionContext context)
        {
            if (context.castExpression() != null)
            {
                throw new NotSupportedException("Not supported yet");
            }

            return Visit(context.unaryExpression());
        }

        public override Expression VisitMultiplicativeExpression(TypeBoxParser.MultiplicativeExpressionContext context)
        {
            if (context.multiplicativeExpression() != null)
            {
                IParseTree oper = context.GetChild(1);

                switch (oper.GetText())
                {
                    case "*":
                        return Expression.Multiply(Visit(context.multiplicativeExpression()), Visit(context.castExpression()));
                    case "/":
                        return Expression.Divide(Visit(context.multiplicativeExpression()), Visit(context.castExpression()));
                    case "%":
                        return Expression.Modulo(Visit(context.multiplicativeExpression()), Visit(context.castExpression()));
                }

                throw new NotSupportedException("Not a supported multiplicative operator");
            }

            return Visit(context.castExpression());
        }

        public override Expression VisitAdditiveExpression(TypeBoxParser.AdditiveExpressionContext context)
        {
            if (context.additiveExpression() != null)
            {
                IParseTree oper = context.GetChild(1);

                switch (oper.GetText())
                {
                    case "+":
                        return Expression.Add(Visit(context.additiveExpression()), Visit(context.multiplicativeExpression()));
                    case "-":
                        return Expression.Subtract(Visit(context.additiveExpression()), Visit(context.multiplicativeExpression()));
                }

                throw new NotSupportedException("Not a supported addative operator");
            }

            return Visit(context.multiplicativeExpression());
        }

        public override Expression VisitShiftExpression(TypeBoxParser.ShiftExpressionContext context)
        {
            if (context.shiftExpression() != null)
            {
                IParseTree oper = context.GetChild(1);

                switch (oper.GetText())
                {
                    case "<<":
                        return Expression.LeftShift(Visit(context.shiftExpression()), Visit(context.additiveExpression()));
                    case ">>":
                        return Expression.RightShift(Visit(context.shiftExpression()), Visit(context.additiveExpression()));
                }

                throw new NotSupportedException("Not a supported equality operator");
            }

            return Visit(context.additiveExpression());
        }

        public override Expression VisitRelationalExpression(TypeBoxParser.RelationalExpressionContext context)
        {
            if (context.relationalExpression() != null)
            {
                IParseTree oper = context.GetChild(1);

                switch (oper.GetText())
                {
                    case "<":
                        return Expression.LessThan(Visit(context.relationalExpression()), Visit(context.shiftExpression()));
                    case ">":
                        return Expression.GreaterThan(Visit(context.relationalExpression()), Visit(context.shiftExpression()));
                    case "<=":
                        return Expression.LessThanOrEqual(Visit(context.relationalExpression()), Visit(context.shiftExpression()));
                    case ">=":
                        return Expression.GreaterThanOrEqual(Visit(context.relationalExpression()), Visit(context.shiftExpression()));
                }

                throw new NotSupportedException("Not a supported relational operator");
            }

            return Visit(context.shiftExpression());
        }

        public override Expression VisitEqualityExpression(TypeBoxParser.EqualityExpressionContext context)
        {
            if (context.equalityExpression() != null)
            {
                IParseTree oper = context.GetChild(1);

                switch (oper.GetText())
                {
                    case "==":
                        return Expression.Equal(Visit(context.equalityExpression()), Visit(context.relationalExpression()));
                    case "!=":
                        return Expression.NotEqual(Visit(context.equalityExpression()), Visit(context.relationalExpression()));
                }

                throw new NotSupportedException("Not a supported equality operator");
            }

            return Visit(context.relationalExpression());
        }

        public override Expression VisitAndExpression(TypeBoxParser.AndExpressionContext context)
        {
            if (context.andExpression() != null)
            {
                return Expression.And(Visit(context.andExpression()), Visit(context.equalityExpression()));
            }

            return Visit(context.equalityExpression());
        }

        public override Expression VisitExclusiveOrExpression(TypeBoxParser.ExclusiveOrExpressionContext context)
        {
            if (context.exclusiveOrExpression() != null)
            {
                return Expression.ExclusiveOr(Visit(context.exclusiveOrExpression()), Visit(context.andExpression()));
            }

            return Visit(context.andExpression());
        }

        public override Expression VisitInclusiveOrExpression(TypeBoxParser.InclusiveOrExpressionContext context)
        {
            if (context.inclusiveOrExpression() != null)
            {
                return Expression.Or(Visit(context.inclusiveOrExpression()), Visit(context.exclusiveOrExpression()));
            }

            return Visit(context.exclusiveOrExpression());
        }

        public override Expression VisitLogicalAndExpression(TypeBoxParser.LogicalAndExpressionContext context)
        {
            if (context.logicalAndExpression() != null)
            {
                return Expression.AndAlso(Visit(context.logicalAndExpression()), Visit(context.inclusiveOrExpression()));
            }

            return Visit(context.inclusiveOrExpression());
        }

        public override Expression VisitLogicalOrExpression(TypeBoxParser.LogicalOrExpressionContext context)
        {
            if (context.logicalOrExpression() != null)
            {
                return Expression.OrElse(Visit(context.logicalOrExpression()), Visit(context.logicalAndExpression()));
            }

            return Visit(context.logicalAndExpression());
        }

        public override Expression VisitConditionalExpression(TypeBoxParser.ConditionalExpressionContext context)
        {
            if (context.expression() != null)
            {
                // We have a conditional expression
                return Expression.Condition(Visit(context.logicalOrExpression()), Visit(context.expression()),
                    Visit(context.conditionalExpression()));
            }

            return Visit(context.logicalOrExpression());
        }

        public override Expression VisitAssignmentExpression(TypeBoxParser.AssignmentExpressionContext context)
        {
            if (context.assignmentOperator() != null)
            {
                var oper = context.GetChild(1).GetText();

                switch (oper)
                {
                    case "=":
                        return Expression.Assign(Visit(context.unaryExpression()), Visit(context.assignmentExpression()));
                    case "*=":
                        return Expression.MultiplyAssign(Visit(context.unaryExpression()), Visit(context.assignmentExpression()));
                    case "/=":
                        return Expression.DivideAssign(Visit(context.unaryExpression()), Visit(context.assignmentExpression()));
                    case "%=":
                        return Expression.ModuloAssign(Visit(context.unaryExpression()), Visit(context.assignmentExpression()));
                    case "+=":
                        return Expression.AddAssign(Visit(context.unaryExpression()), Visit(context.assignmentExpression()));
                    case "-=":
                        return Expression.SubtractAssign(Visit(context.unaryExpression()), Visit(context.assignmentExpression()));
                    case "<<=":
                        return Expression.LeftShiftAssign(Visit(context.unaryExpression()), Visit(context.assignmentExpression()));
                    case ">>=":
                        return Expression.RightShiftAssign(Visit(context.unaryExpression()), Visit(context.assignmentExpression()));
                    case "&=":
                        return Expression.AndAssign(Visit(context.unaryExpression()), Visit(context.assignmentExpression()));
                    case "^=":
                        return Expression.ExclusiveOrAssign(Visit(context.unaryExpression()), Visit(context.assignmentExpression()));
                    case "|=":
                        return Expression.OrAssign(Visit(context.unaryExpression()), Visit(context.assignmentExpression()));
                }

                throw new NotSupportedException("Not a supported assignment operator");
            }

            return Visit(context.conditionalExpression());
        }

        public override Expression VisitExpression(TypeBoxParser.ExpressionContext context)
        {
            if (context.expression() != null)
            {
                throw new NotSupportedException("Can't handle this");
            }

            return Visit(context.assignmentExpression());
        }

        public override Expression VisitConstant(TypeBoxParser.ConstantContext context)
        {
            if (context.IntegerConstant() != null)
            {
                return Expression.Constant(int.Parse(context.IntegerConstant().GetText()));
            }

            if (context.FloatConstant() != null)
            {
                return Expression.Constant(double.Parse(context.FloatConstant().GetText(), System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
            }

            if (context.BooleanConstant() != null)
            {
                return Expression.Constant(bool.Parse(context.BooleanConstant().GetText()));
            }

            throw new NotSupportedException("Constant type is not supported");
        }
    }
}
