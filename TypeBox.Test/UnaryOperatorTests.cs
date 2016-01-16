using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TypeBox.Test
{
    [TestClass]
    public class UnaryOperatorTests
    {
        private TypeBoxScriptEngine _luScriptEngine;

        [TestInitialize]
        public void Initialize()
        {
            _luScriptEngine = new TypeBoxScriptEngine();
        }

        [TestMethod]
        public void NotTrueShallBeFalse()
        {
            AssertEval(false, "!true");
        }

        [TestMethod]
        public void NotFalseShallBeTrue()
        {
            AssertEval(true, "!false");
        }

        [TestMethod]
        public void NotTrueVariableShallBeFalse()
        {
            var environment = new DefaultTestEnvironment {BoolVar = true};
            AssertEval(environment, false, "!BoolVar");
        }

        [TestMethod]
        public void NotFalseVariableShallBeTrue()
        {
            var environment = new DefaultTestEnvironment { BoolVar = false };
            AssertEval(environment, true, "!BoolVar");
        }

        private void AssertEval<T>(T expected, string expression)
        {
            Assert.AreEqual(expected, _luScriptEngine.Evaluate<T>(expression));
        }

        private void AssertEval<T>(DefaultTestEnvironment environment, T expected, string expression)
        {
            Assert.AreEqual(expected, _luScriptEngine.Evaluate<T, DefaultTestEnvironment>(environment, expression));
        }
    }
}
