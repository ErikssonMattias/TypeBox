using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TypeBox.Test
{
    [TestClass]
    public class ConversionTests
    {
        private TypeBoxScriptEngine _typeBoxScriptEngine;

        [TestInitialize]
        public void Initialize()
        {
            _typeBoxScriptEngine = new TypeBoxScriptEngine();
        }

        [TestMethod]
        public void ConvertTypeBoxArrayToArray()
        {
            DefaultTestEnvironment env = new DefaultTestEnvironment();

            var code = _typeBoxScriptEngine.Compile<DefaultTestEnvironment>(@"
var hej : number[] = [];
hej.push(2);
hej.push(3.2);

DoubleArray = hej;
");

            code(env);

            Assert.IsNotNull(env.DoubleArray);
            Assert.AreEqual(2, env.DoubleArray.Length);
            Assert.AreEqual(2, env.DoubleArray[0]);
            Assert.AreEqual(3.2, env.DoubleArray[1]);
        }

        [TestMethod]
        public void ConvertTypeBoxDoubleArrayToIntArray()
        {
            DefaultTestEnvironment env = new DefaultTestEnvironment();

            var code = _typeBoxScriptEngine.Compile<DefaultTestEnvironment>(@"
var hej : number[] = [];
hej.push(2);
hej.push(3.2);

IntArray = hej;
");

            code(env);

            Assert.IsNotNull(env.IntArray);
            Assert.AreEqual(2, env.IntArray.Length);
            Assert.AreEqual(2, env.IntArray[0]);
            Assert.AreEqual(3, env.IntArray[1]);
        }

        [TestMethod]
        public void DoubleArrayToIntEnumerable()
        {
            DefaultTestEnvironment env = new DefaultTestEnvironment();

            env.DoubleArray = new[] {3.4, 5.7};

            var code = _typeBoxScriptEngine.Compile<DefaultTestEnvironment>(@"
IntEnumerable = DoubleArray;
");

            code(env);

            Assert.IsNotNull(env.IntEnumerable);
            //Assert.IsInstanceOfType(env.IntEnumerable, typeof(TypeBoxArray));
            var intArr = env.IntEnumerable.ToArray();
            Assert.AreEqual(2, intArr.Length);
            Assert.AreEqual(3, intArr[0]);
            Assert.AreEqual(6, intArr[1]);
        }

        [TestMethod]
        public void DoubleArrayToIntList()
        {
            DefaultTestEnvironment env = new DefaultTestEnvironment();

            env.DoubleArray = new[] { 3.4, 5.7 };

            var code = _typeBoxScriptEngine.Compile<DefaultTestEnvironment>(@"
IntList = DoubleArray;
");

            code(env);

            Assert.IsNotNull(env.IntList);
            Assert.AreEqual(2, env.IntList.Count);
            Assert.AreEqual(3, env.IntList[0]);
            Assert.AreEqual(6, env.IntList[1]);
        }
    }
}
