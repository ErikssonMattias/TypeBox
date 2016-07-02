using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TypeBox.Test
{
    [TestClass]
    public class ArrayTests
    {
        private TypeBoxScriptEngine _typeBoxScriptEngine;

        [TestInitialize]
        public void Initialize()
        {
            _typeBoxScriptEngine = new TypeBoxScriptEngine();
        }

        [TestMethod]
        public void DeclareArray()
        {
            DefaultTestEnvironment env = new DefaultTestEnvironment();

            var code = _typeBoxScriptEngine.Compile<DefaultTestEnvironment>(@"
var hej : number[];
Result = 1337;
");

            code(env);

            Assert.AreEqual(1337, env.Result);
        }

        [TestMethod]
        public void DeclareArrayGeneric()
        {
            DefaultTestEnvironment env = new DefaultTestEnvironment();

            var code = _typeBoxScriptEngine.Compile<DefaultTestEnvironment>(@"
var hej : Array<number> = [];
hej.push(4.1);
Result = 1337;
");

            code(env);

            Assert.AreEqual(1337, env.Result);
        }

        [TestMethod]
        public void ArrayPush()
        {
            DefaultTestEnvironment env = new DefaultTestEnvironment();

            var code = _typeBoxScriptEngine.Compile<DefaultTestEnvironment>(@"
var hej : number[] = [];
hej.push(3.1);
hej.push(4.5);
DoubleVar = 0.0;
for (var i of hej) {
    DoubleVar += i;
}
");

            code(env);

            Assert.AreEqual(7.6, env.DoubleVar);
        }
    }
}
