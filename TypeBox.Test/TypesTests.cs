using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TypeBox.Test
{
    [TestClass]
    public class TypesTests
    {
        private TypeBoxScriptEngine _typeBoxScriptEngine;

        [TestInitialize]
        public void Initialize()
        {
            _typeBoxScriptEngine = new TypeBoxScriptEngine();
        }

        [TestMethod]
        public void CustomType()
        {
            DefaultTestEnvironment env = new DefaultTestEnvironment();

            env.IntList = new List<int>();


            var code = _typeBoxScriptEngine.Compile<DefaultTestEnvironment>(new [] {typeof(List<>)}, @"
var hej : List<int>;
hej = IntList;
hej.Add(3);
");

            code(env);

            Assert.AreEqual(1, env.IntList.Count);
            Assert.AreEqual(3, env.IntList[0]);
        }

        [TestMethod]
        public void NewKeyword()
        {
            DefaultTestEnvironment env = new DefaultTestEnvironment();

            env.IntList = new List<int>();


            var code = _typeBoxScriptEngine.Compile<DefaultTestEnvironment>(new[] { typeof(SubEnvironment) }, @"

ObjectInstance = new SubEnvironment();
");

            code(env);

            
            Assert.IsInstanceOfType(env.ObjectInstance, typeof(SubEnvironment));
        }

        [TestMethod]
        public void CreateObjectCallMethod()
        {
            DefaultTestEnvironment env = new DefaultTestEnvironment();

            env.IntList = new List<int>();


            var code = _typeBoxScriptEngine.Compile<DefaultTestEnvironment>(new[] { typeof(SubEnvironment) }, @"

var hej = new SubEnvironment();
hej.SetKalle(6);

ObjectInstance = hej;
");

            code(env);


            Assert.IsInstanceOfType(env.ObjectInstance, typeof(SubEnvironment));
            var subEnv = env.ObjectInstance as SubEnvironment;
            Assert.AreEqual(6, subEnv.Kalle);
        }
    }
}
