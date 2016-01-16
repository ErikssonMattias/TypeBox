using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TypeBox.Test
{
    [TestClass]
    public class EnvironmentTests
    {
        private TypeBoxScriptEngine _typeBoxScriptEngine;

        [TestInitialize]
        public void Initialize()
        {
            _typeBoxScriptEngine = new TypeBoxScriptEngine();
        }

        [TestMethod]
        public void EnvironmentAccess()
        {
            DefaultTestEnvironment env = new DefaultTestEnvironment();

            var code = _typeBoxScriptEngine.Compile<DefaultTestEnvironment>(@"
int hej = 2;
SubEnv.Kalle = 3;
Result = SubEnv.Kalle + hej;
SubEnv.SetKalle(hej + 7);");

            code(env);


            Assert.AreEqual(5, env.Result);
            Assert.AreEqual(9, env.SubEnv.Kalle);
        }
    }
}
