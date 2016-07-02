using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TypeBox.Test
{
    [TestClass]
    public class CastTests
    {
        private TypeBoxScriptEngine _typeBoxScriptEngine;

        [TestInitialize]
        public void Initialize()
        {
            _typeBoxScriptEngine = new TypeBoxScriptEngine();
        }

        [TestMethod]
        public void NumericCasts()
        {
            DefaultTestEnvironment env = new DefaultTestEnvironment();

            var code = _typeBoxScriptEngine.Compile<DefaultTestEnvironment>(@"
var hej : number;
hej = 2; // Assign integer to double
Result = hej; // Assign double to integer
");

            code(env);

            Assert.AreEqual(2, env.Result);
        }
    }
}
