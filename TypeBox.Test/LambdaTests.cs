using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TypeBox.Test
{
    [TestClass]
    public class LambdaTests
    {
        private class LambdaTestEnvironment
        {
            public int Result = 0;

            public Func<int, int> FuncIntInIntOut;

            public event Action<int> EventWithInt;

            public void FireEventWithInt(int i)
            {
                EventWithInt?.Invoke(i);
            }
        }

        private TypeBoxScriptEngine _typeBoxScriptEngine;

        [TestInitialize]
        public void Initialize()
        {
            _typeBoxScriptEngine = new TypeBoxScriptEngine();
        }

        [TestMethod]
        public void EventLambdaWithoutType()
        {
            var environment = new LambdaTestEnvironment();
            _typeBoxScriptEngine.Execute(@"
EventWithInt += (hej) => { Result = hej; };
", environment);

            Assert.AreEqual(0, environment.Result);
            environment.FireEventWithInt(7);
            Assert.AreEqual(7, environment.Result);
        }

        [TestMethod]
        [ExpectedException(typeof(TypeBoxParseException))]
        public void EventLambdaWithWrongType()
        {
            var environment = new LambdaTestEnvironment();
            _typeBoxScriptEngine.Execute(@"
EventWithInt += (hej : double) => { Result = hej; };
", environment);

            Assert.AreEqual(0, environment.Result);
            environment.FireEventWithInt(7);
            Assert.AreEqual(7, environment.Result);
        }

        [TestMethod]
        public void AssignLambdaWithoutType()
        {
            var environment = new LambdaTestEnvironment();
            _typeBoxScriptEngine.Execute(@"
FuncIntInIntOut = (hej) => { return hej + 2; };
", environment);

            Assert.AreEqual(0, environment.Result);
            Assert.IsNotNull(environment.FuncIntInIntOut);
            Assert.AreEqual(9, environment.FuncIntInIntOut(7));
        }

        [TestMethod]
        [ExpectedException(typeof(TypeBoxParseException))]
        public void AssignLambdaWithWrongReturnType()
        {
            var environment = new LambdaTestEnvironment();
            _typeBoxScriptEngine.Execute(@"
FuncIntInIntOut = (hej) : double => { return hej + 2; };
", environment);
            
        }

        [TestMethod]
        public void AssignLambdaWithSpecifiedType()
        {
            var environment = new LambdaTestEnvironment();
            _typeBoxScriptEngine.Execute(@"
FuncIntInIntOut = (hej : int) : int => { return hej + 2; };
", environment);

            Assert.AreEqual(0, environment.Result);
            Assert.IsNotNull(environment.FuncIntInIntOut);
            Assert.AreEqual(9, environment.FuncIntInIntOut(7));
        }

        [TestMethod]
        public void NullSafeLambdaCall()
        {
            var environment = new LambdaTestEnvironment();
            environment.FuncIntInIntOut = null;

            _typeBoxScriptEngine.Execute(@"
FuncIntInIntOut?(5);
", environment);
        }
    }
}
