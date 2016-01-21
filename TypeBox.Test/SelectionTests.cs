using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TypeBox.Test
{
    [TestClass]
    public class SelectionTests
    {
        private TypeBoxScriptEngine _typeBoxScriptEngine;

        [TestInitialize]
        public void Initialize()
        {
            _typeBoxScriptEngine = new TypeBoxScriptEngine();
        }

        [TestMethod]
        public void IfThen()
        {
            DefaultTestEnvironment env = new DefaultTestEnvironment();

            var code = _typeBoxScriptEngine.Compile<DefaultTestEnvironment>(@"
Result = 0;
if (true) Result = 1;
if (false) Result = 2;");

            code(env);

            Assert.AreEqual(1, env.Result);
        }

        [TestMethod]
        public void IfThenElse()
        {
            DefaultTestEnvironment env = new DefaultTestEnvironment();

            var code = _typeBoxScriptEngine.Compile<DefaultTestEnvironment>(@"
Result = 0;
if (BoolVar) Result = 1;
else Result = 2;");

            env.BoolVar = true;
            code(env);

            Assert.AreEqual(1, env.Result);

            env.BoolVar = false;
            code(env);
            Assert.AreEqual(2, env.Result);
        }
    }
}
