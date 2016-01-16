using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TypeBox.Test
{
    [TestClass]
    public class CommentTests
    {
        private TypeBoxScriptEngine _typeBoxScriptEngine;

        [TestInitialize]
        public void Initialize()
        {
            _typeBoxScriptEngine = new TypeBoxScriptEngine();
        }

        [TestMethod]
        public void LineComment()
        {
            var prog = _typeBoxScriptEngine.Compile<DefaultTestEnvironment, Object>(@"
Result = 1; // Result = 5;
Result = Result + 1;");

            var environment = new DefaultTestEnvironment();
            prog(environment, new object());

            Assert.AreEqual(2, environment.Result);
        }

        [TestMethod]
        public void BlockComment()
        {
            var prog = _typeBoxScriptEngine.Compile<DefaultTestEnvironment, Object>(@"
Result = 1; /* Result = 5;
Result = 3;
Result = Result + 1*/;");

            var environment = new DefaultTestEnvironment();
            prog(environment, new object());

            Assert.AreEqual(1, environment.Result);
        }
    }
}
