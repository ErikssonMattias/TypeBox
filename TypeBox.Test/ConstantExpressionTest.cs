using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TypeBox.Test
{
    [TestClass]
    public class ConstantExpressionTest
    {
        private TypeBoxScriptEngine _typeBoxScriptEngine;

        [TestInitialize]
        public void Initialize()
        {
            _typeBoxScriptEngine = new TypeBoxScriptEngine();
        }

        [TestMethod]
        public void AddOne()
        {
            AssertEval(3, "1 + 2");
        }


        [TestMethod]
        public void SubOne()
        {
            AssertEval(-1, "1 - 2");
        }

        [TestMethod]
        public void MultiplyInteger()
        {
            AssertEval(20, "4 * 5");
        }

        [TestMethod]
        public void MultiplyFloat()
        {
            AssertEval(25.2, "4.5 * 5.6");
        }

        [TestMethod]
        public void DivideInteger()
        {
            AssertEval(5, "30 / 6");
        }

        [TestMethod]
        public void DivideFloat()
        {
            AssertEval(5.6, "25.2 / 4.5");
        }

        [TestMethod]
        public void Or1()
        {
            AssertEval(false, "false || false");
        }

        [TestMethod]
        public void Or2()
        {
            AssertEval(true, "true || false");
        }

        [TestMethod]
        public void Or3()
        {
            AssertEval(true, "false || true");
        }

        [TestMethod]
        public void And1()
        {
            AssertEval(false, "false && false");
        }

        [TestMethod]
        public void And2()
        {
            AssertEval(false, "true && false");
        }

        [TestMethod]
        public void And3()
        {
            AssertEval(true, "true && true");
        }

        [TestMethod]
        public void GreaterThanPositive()
        {
            AssertEval(true, "5 > 2");
        }

        [TestMethod]
        public void GreaterThanNegative1()
        {
            AssertEval(false, "3 > 5");
        }

        [TestMethod]
        public void GreaterThanNegative2()
        {
            AssertEval(false, "5 > 5");
        }

        [TestMethod]
        public void LessThanPositive()
        {
            AssertEval(true, "2 < 5");
        }

        [TestMethod]
        public void LessThanNegative1()
        {
            AssertEval(false, "5 < 3");
        }

        [TestMethod]
        public void LessThanNegative2()
        {
            AssertEval(false, "5 < 5");
        }

        private void AssertEval<T>(T expected, string expression)
        {
            Assert.AreEqual(expected, _typeBoxScriptEngine.Evaluate<T>(expression));
        }
    }
}
