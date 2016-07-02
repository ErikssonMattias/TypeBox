using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TypeBox.Test
{
    [TestClass]
    public class FunctionTests
    {
        private TypeBoxScriptEngine _typeBoxScriptEngine;

        [TestInitialize]
        public void Initialize()
        {
            _typeBoxScriptEngine = new TypeBoxScriptEngine();
        }

        [TestMethod]
        public void DefineFunction()
        {
            _typeBoxScriptEngine.Execute(@"
function func(hej : number) : void {
    hej = 33;
}
");
        }

        [TestMethod]
        public void CallFunction()
        {
            var environment = new DefaultTestEnvironment {Result = 0};
            _typeBoxScriptEngine.Execute(@"
function func() : void {
    Result = 33;
}

func();
", environment);

            Assert.AreEqual(33, environment.Result);
        }

        [TestMethod]
        public void FunctionLocalScope()
        {
            var environment = new DefaultTestEnvironment { Result = 0 };
            _typeBoxScriptEngine.Execute(@"
function func() : void {
    var Result : int = 33;
}

func();
        ", environment);

            Assert.AreNotEqual(33, environment.Result);
        }

        [TestMethod]
        [ExpectedException(typeof(TypeBoxParseException), "A function-local variable was accessible outside its scope.")]
        public void FunctionNestedLocalScope()
        {
            var environment = new DefaultTestEnvironment { Result = 0 };
            _typeBoxScriptEngine.Execute(@"
function func() : void {
    var out = 33;
}

func();

Result = out;
        ", environment);
            
            Assert.AreNotEqual(33, environment.Result);
        }

        [TestMethod]
        public void FunctionWithArguments()
        {
            var environment = new DefaultTestEnvironment { Result = 0 };
            _typeBoxScriptEngine.Execute(@"
function add(left : int, right : int) : void {
    Result = left + right;
}

add(3, 4);
        ", environment);

            Assert.AreEqual(7, environment.Result);
        }

        [TestMethod]
        public void FunctionWithReturnValue()
        {
            var environment = new DefaultTestEnvironment { Result = 0 };
            _typeBoxScriptEngine.Execute(@"
function add(left : int, right : int) : int {
    return left + right;
}

Result = add(3, 9);
        ", environment);

            Assert.AreEqual(12, environment.Result);
        }

        [TestMethod]
        public void FunctionWithObjectReturnValue()
        {
            var environment = new DefaultTestEnvironment { Result = 0 };
            _typeBoxScriptEngine.Execute(@"
function add(left : int, right : int) : object {
    return ClassInstance;
}

ObjectInstance = add(3, 4);
        ", environment);

            Assert.AreSame(environment.ClassInstance, environment.ObjectInstance);
        }

        //        [TestMethod]
        //        public void LuFunctionShallNotBeConvertedIfNotNeeded()
        //        {
        //            _environment.Dynamic.invoke = new Func<object, object>(Invoke);
        //            _typeBoxScriptEngine.Execute(@"
        //function add() 
        //    return 1 + 2
        //end

        //out = invoke(add)
        //", _environment);

        //            Assert.AreEqual(3, _environment.Dynamic.@out);
        //        }

        [TestMethod]
        public void FunctionAsExpression()
        {
            var environment = new DefaultTestEnvironment { Result = 0 };
            _typeBoxScriptEngine.Execute(@"
var func = () : void => {
};
ObjectInstance = func;
"
            , environment);

            Assert.IsInstanceOfType(environment.ObjectInstance, typeof(Action));
        }

        [TestMethod]
        public void FunctionAsExpressionWithParamsAndReturn()
        {
            var environment = new DefaultTestEnvironment { Result = 0 };
            _typeBoxScriptEngine.Execute(@"
var func = (str : double) : int => {
    return 1 + 2;
};
ObjectInstance = func;
"
            , environment);

            Assert.IsInstanceOfType(environment.ObjectInstance, typeof(Func<double, int>));
        }

        //        [TestMethod]
        //        public void FunctionConvertedToFunc()
        //        {
        //            var plusOne = _typeBoxScriptEngine.Evaluate<Func<int, int>>(@"
        //function(input) 
        //    return input + 1
        //end", _environment);

        //            Assert.AreEqual(3, plusOne(2));
        //        }

        //        private static object Invoke(object func)
        //        {
        //            dynamic dfunc = func;

        //            // Some superfluouse arguments that a LuFunction should handle by ignoring
        //            return dfunc(123, 123);
        //        }

        [TestMethod]
        public void LambdaToEnvironment()
        {
            var environment = new DefaultTestEnvironment { Result = 0 };
            _typeBoxScriptEngine.Execute(@"

FuncIntInIntOut = (param : int) : int => {
    Result = param;
    return param + 1;
};
"
            , environment);
            

            Assert.IsInstanceOfType(environment.FuncIntInIntOut, typeof(Func<int, int>));

            var funcResult = environment.FuncIntInIntOut(5);

            Assert.AreEqual(6, funcResult);
            Assert.AreEqual(5, environment.Result);
        }

        [TestMethod]
        public void SubscribeEventWithLambda()
        {
            var environment = new DefaultTestEnvironment { Result = 0 };
            _typeBoxScriptEngine.Execute(@"

EventWithInt += (hej : int) : void => { Result = hej; };
"
            , environment);


            Assert.AreEqual(0, environment.Result);

            environment.FireEventWithInt(8);

            Assert.AreEqual(8, environment.Result);
        }
    }
}
