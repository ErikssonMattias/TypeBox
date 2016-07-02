﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Antlr4.Runtime.Tree;
using TypeBox.InternalTypes;
using TypeBox.Parser;

namespace TypeBox.Compilation
{
    internal partial class Visitor
    {
        private TypeBoxArray<int> _emptyArray = new TypeBoxArray<int>();

        private bool IsIntegerType(Type type)
        {
            if (type == typeof(int) || type == typeof(long) || type == typeof(uint) || type == typeof(byte) || type == typeof(short) || type == typeof(ushort) || type == typeof(ulong))
            {
                return true;
            }

            return false;
        }

        private bool IsFloatType(Type type)
        {
            if (type == typeof(double) || type == typeof(float))
            {
                return true;
            }

            return false;
        }

        private bool IsNumberType(Type type)
        {
            return IsIntegerType(type) || IsFloatType(type);
        }

        private Expression CastAssignment(Type leftType, Expression right)
        {
            if (leftType == right.Type)
            {
                return right;
            }

            if (IsNumberType(leftType) && IsNumberType(right.Type))
            {
                return Expression.Convert(right, leftType);
            }

            // "Array"/List conversions (only works woth 1 dimensional lists at the moment)
            if (EnumerationConverter.IsAssignableEnumeration(leftType) && right.Type.ImplementsIEnumerable())
            {
                var leftElementType = EnumerationConverter.GetElementType(leftType);
                var rightElementType = EnumerationConverter.GetElementType(right.Type);
                //var call = Expression.Convert(Expression.Call(null, typeof(EnumerationConverter).GetMethod("AssignEnumeration").MakeGenericMethod(leftType, leftElementType, rightElementType), right), leftType);

                //right = call;
                //CastAssignment(leftType, ref right);
                return Expression.Convert(Expression.Call(null, typeof(EnumerationConverter).GetMethod("AssignEnumeration").MakeGenericMethod(leftType, leftElementType, rightElementType), right), leftType);
            }

            //if (leftType.IsArray || leftType.IsEqualGenericType(typeof(IEnumerable<>)))
            //{
            //    var leftElementType = leftType.GetElementType();

            //    if (right.Type.IsEqualGenericType(typeof(TypeBoxArray<>)))
            //    {
            //        var rightElementType = right.Type.GetFirstGenericArgument();
            //        if (leftElementType == rightElementType)
            //        {
            //            right = Expression.Call(right, right.Type.GetMethod("ToArray"));
            //            return;
            //        }

            //        if (IsNumberType(leftElementType) && IsNumberType(rightElementType))
            //        {
            //            right = Expression.Call(right, right.Type.GetMethod("ChangeTypeOfMembersToArray").MakeGenericMethod(leftElementType));
            //            return;
            //        }

            //        right = Expression.Call(right, right.Type.GetMethod("CastToArray").MakeGenericMethod(leftElementType));
            //        return;
            //    }

            //    if (right.Type.IsArray)
            //    {
            //        var rightElementType = right.Type.GetElementType();
            //        int[] hej;

            //    }
            //}

            // Check is assignment of "The Empty Array" to an array. If so, initialize the array with the correct type
            ConstantExpression rightConstant = right as ConstantExpression;

            if ((leftType.IsGenericType) && (leftType.GetGenericTypeDefinition() == typeof(TypeBoxArray<>)) && (rightConstant != null) &&
                        (rightConstant.Value == _emptyArray))
            {
                return Expression.Constant(Activator.CreateInstance(leftType));
            }

            return right;
        }

        public override Expression VisitLambdaExpression(TypeBoxParser.LambdaExpressionContext context)
        {
            //if (context.initDeclaratorList() != null)
            //{
            //    _scope = new Scope(_scope);

            //    var parameters = GetInitDeclaratorList(context.initDeclaratorList());

            //    var block = VisitBlock(context.block());

            //    _scope = _scope.Parent;
                
            //    return Expression.Lambda(block, parameters);
            //}

            return GetLambdaExpression(context.compoundStatement(), context.typeSpecifier(), context.initDeclaratorList(), null);
        }

        public override Expression VisitNewExpression(TypeBoxParser.NewExpressionContext context)
        {
            var type = GetTypeFromTypeSpecifier(context.typeSpecifier());
            
            var argumentList = GetArgumentExpressionList(context.argumentExpressionList());
            var typeArray = argumentList.Select(x => x.Type).ToArray();

            var constructorInfo = type.GetConstructor(typeArray);

            if (constructorInfo != null)
            {
                return Expression.New(constructorInfo, argumentList);
            }

            throw new TypeBoxCompileException($"Could not find a matching constructor for type {type}");
        }

        public override Expression VisitPrimaryExpression(TypeBoxParser.PrimaryExpressionContext context)
        {
            if (context.constant() != null)
            {
                return Visit(context.constant());
            }

            if (context.lambdaExpression() != null)
            {
                return VisitLambdaExpression(context.lambdaExpression());
            }

            if (context.newExpression() != null)
            {
                return VisitNewExpression(context.newExpression());
            }

            var name = context.NAME().GetText();
            var expression = _scope.Get(name);

            if (expression == null)
            {
                ParameterExpression instance;
                var memberInfos = _scope.GetMemberInfos(name, out instance);
            }

            return expression;
        }
        
        public override Expression VisitArrayPostfixExpression(TypeBoxParser.ArrayPostfixExpressionContext context)
        {
            return base.VisitArrayPostfixExpression(context);
        }

        public override Expression VisitPostfixPrimaryExpression(TypeBoxParser.PostfixPrimaryExpressionContext context)
        {
            return Visit(context.primaryExpression());
        }

        public override Expression VisitFunctionCall(TypeBoxParser.FunctionCallContext context)
        {
            var func = Visit(context.postfixExpression());

            var methodInfosExpression = func as MethodInfosExpression;

            if (methodInfosExpression != null)
            {
                // If a function in the root of an environment is called, it will be disguised as a fonction call

                var argumentList = GetArgumentExpressionList(context.argumentExpressionList()).ToArray();
                
                var methodInfos = methodInfosExpression.MethodInfos.ToArray();

                if (methodInfos.Length > 1)
                {
                    throw new TypeBoxCompileException($"Does not support overloaded method calling at the moment.");
                }
                
                var methodInfo = methodInfos[0].MethodInfo;
                var instance = methodInfos[0].Instance;

                var parameterInfo = methodInfo.GetParameters();
                for (int i = 0; i < argumentList.Length; i++)
                {
                    argumentList[i] = CastAssignment(parameterInfo[i].ParameterType, argumentList[i]);
                }

                return Expression.Call(instance, methodInfo, argumentList);
            }
            
            return Expression.Invoke(func, GetArgumentExpressionList(context.argumentExpressionList()));
        }

        public override Expression VisitMemberAccessExpression(TypeBoxParser.MemberAccessExpressionContext context)
        {
            string oper = context.GetChild(1).GetText();

            
            var instance = Visit(context.postfixExpression());
            var member = context.NAME().GetText();

            Expression memberExpression = null;
            try
            {
                memberExpression = Expression.PropertyOrField(instance, member);
            }
            catch (ArgumentException)
            {
                
            }

            if (memberExpression != null)
            {
                if (_settings.NullSafeMemberAccess || (oper == "?." && _settings.NullSafeMemberAccessOperator))
                {
                    var binaryExpression = Expression.Equal(
                        instance, 
                        Expression.Constant(null)
                        );

                    return Expression.Condition(
                        binaryExpression,
                        Expression.Constant(null, memberExpression.Type), 
                        memberExpression, memberExpression.Type);
                }

                return memberExpression;
            }

            var eventInfo = instance.Type.GetEvent(member);
            if (eventInfo != null)
            {
                return new EventInfoExpression(eventInfo, instance);
            }

            var methodInfos = instance.Type.GetMethods().Where(x => x.Name == member).ToArray();
            if (methodInfos.Length > 0)
            {
                return new MethodInfosExpression(methodInfos.Select(x => new MethodInfosExpression.MethodInfoInstancePair {Instance = instance, MethodInfo = x}));
            }

            throw new TypeBoxCompileException($"Cannot find a member with name '{member}'");
        }

        public override Expression VisitPostIncrementExpression(TypeBoxParser.PostIncrementExpressionContext context)
        {
            return Expression.PostIncrementAssign(Visit(context.postfixExpression()));
        }

        public override Expression VisitPostDecrementExpression(TypeBoxParser.PostDecrementExpressionContext context)
        {
            return Expression.PostDecrementAssign(Visit(context.postfixExpression()));
        }

        //public override Expression VisitMethodCall(TypeBoxParser.MethodCallContext context)
        //{
        //    var instance = Visit(context.postfixExpression());
        //    var argumentList = GetArgumentExpressionList(context.argumentExpressionList()).ToArray();
        //    var typeList = argumentList.Select(x => x.Type).ToArray();

        //    string name = context.NAME().GetText();
        //    var methodInfo = instance.Type.GetMethod(name, typeList);

        //    if (methodInfo == null)
        //    {
        //        throw new TypeBoxCompileException(
        //            $"Could not found a matching function with name '{name}' and {argumentList.Length} arguments");
        //    }

        //    var parameterInfo = methodInfo.GetParameters();
        //    for (int i = 0; i < argumentList.Length; i++)
        //    {
        //        argumentList[i] = CastAssignment(parameterInfo[i].ParameterType, argumentList[i]);
        //    }

        //    return Expression.Call(instance, methodInfo, argumentList);
        //}
        
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

                Expression left = Visit(context.unaryExpression());
                Expression right = Visit(context.assignmentExpression());

                if ((left is TypeBoxExpression) && !(left is EventInfoExpression))
                {
                    throw new TypeBoxCompileException("Cannot assign to member of that type.");
                }

                var eventInfoExpression = left as EventInfoExpression;

                switch (oper)
                {
                    case "+=":
                        if (eventInfoExpression != null)
                        {
                            return Expression.Call(eventInfoExpression.Instance, eventInfoExpression.EventInfo.AddMethod, right);
                        }

                        return Expression.AddAssign(left, right);

                    case "-=":
                        if (eventInfoExpression != null)
                        {
                            return Expression.Call(eventInfoExpression.Instance, eventInfoExpression.EventInfo.RemoveMethod, right);
                        }

                        return Expression.SubtractAssign(left, right);
                }

                if (left is TypeBoxExpression)
                {
                    throw new TypeBoxCompileException("Cannot assign to member of that type.");
                }

                switch (oper)
                {
                    case "=":
                        return Expression.Assign(left, CastAssignment(left.Type, right));
                    case "*=":
                        return Expression.MultiplyAssign(left, right);
                    case "/=":
                        return Expression.DivideAssign(left, right);
                    case "%=":
                        return Expression.ModuloAssign(left, right);
                    case "<<=":
                        return Expression.LeftShiftAssign(left, right);
                    case ">>=":
                        return Expression.RightShiftAssign(left, right);
                    case "&=":
                        return Expression.AndAssign(left, right);
                    case "^=":
                        return Expression.ExclusiveOrAssign(left, right);
                    case "|=":
                        return Expression.OrAssign(left, right);
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

        public IList<Expression> GetExpressionList(TypeBoxParser.ExpressionListContext context)
        {
            var expressionList = context.expressionList() != null ? GetExpressionList(context.expressionList()) : new List<Expression>();
            expressionList.Add(VisitAssignmentExpression(context.assignmentExpression()));

            return expressionList;
        }

        public override Expression VisitExpression2(TypeBoxParser.Expression2Context context)
        {
            // The reason for this is to make it easy to destinguish two optional expressions in a for-loop
            return VisitExpression(context.expression());
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

            if (context.arrayConstant() != null)
            {
                if (context.arrayConstant().expressionList() == null)
                {
                    return Expression.Constant(_emptyArray);
                }
                
            }

            if (context.ObjectConstant() != null)
            {
                return Expression.Constant(null);
            }

            throw new NotSupportedException("Constant type is not supported");
        }
    }
}
