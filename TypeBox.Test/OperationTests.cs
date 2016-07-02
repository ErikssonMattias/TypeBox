using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TypeBox.Test
{
    [TestClass]
    public class OperationTests
    {
        private TypeBoxScriptEngine _typeBoxScriptEngine;

        [TestInitialize]
        public void Initialize()
        {
            _typeBoxScriptEngine = new TypeBoxScriptEngine();
        }

        [TestMethod]
        public void NullSafeMemberAccess()
        {
            DefaultTestEnvironment env = new DefaultTestEnvironment();

            env.SubEnv = null;
            env.ObjectInstance = new object();

            var code = _typeBoxScriptEngine.Compile<DefaultTestEnvironment>(@"
ObjectInstance = SubEnv?.SubEnv;
");

            code(env);
            
            Assert.AreEqual(null, env.ObjectInstance);
        }

        [TestMethod]
        public void NullSafeMethodAccess()
        {
            DefaultTestEnvironment env = new DefaultTestEnvironment();

            env.SubEnv = null;
            env.ObjectInstance = new object();

            var code = _typeBoxScriptEngine.Compile<DefaultTestEnvironment>(@"
SubEnv?.SetKalle(5);
");

            code(env);

            Assert.AreEqual(null, env.ObjectInstance);
        }
    }
}
