using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TypeBox.Test
{
    [TestClass]
    public class IterationTests
    {
        private TypeBoxScriptEngine _typeBoxScriptEngine;

        [TestInitialize]
        public void Initialize()
        {
            _typeBoxScriptEngine = new TypeBoxScriptEngine();
        }

        [TestMethod]
        public void SimpleFor()
        {
            DefaultTestEnvironment env = new DefaultTestEnvironment();

            var code = _typeBoxScriptEngine.Compile<DefaultTestEnvironment>(@"
Result = 0;
for(var i = 0; i<3; i++) {
    Result += i;
}
");

            code(env);


            Assert.AreEqual(3, env.Result);
        }

        [TestMethod]
        public void ForWithBreak()
        {
            DefaultTestEnvironment env = new DefaultTestEnvironment();

            var code = _typeBoxScriptEngine.Compile<DefaultTestEnvironment>(@"
Result = 0;
for(var i = 0; ; i++) {
    if (i == 100) {
        break;
    }

    Result = i;
}
");

            code(env);

            Assert.AreEqual(99, env.Result);
        }

        [TestMethod]
        public void ForWithContinueAndBreak()
        {
            DefaultTestEnvironment env = new DefaultTestEnvironment();

            var code = _typeBoxScriptEngine.Compile<DefaultTestEnvironment>(@"
Result = 0;
for(var i = 0; ; i++) {
    if (i == 100) {
        break;
    }
    else {
        continue;
    }

    Result = i;
}
");

            code(env);

            Assert.AreEqual(0, env.Result);
        }

        [TestMethod]
        public void WhileLoop()
        {
            DefaultTestEnvironment env = new DefaultTestEnvironment();

            var code = _typeBoxScriptEngine.Compile<DefaultTestEnvironment>(@"
Result = 0;
var i = 0;
while (i<5) {
    Result += i;
    i++;
}
");

            code(env);

            Assert.AreEqual(10, env.Result);
        }

        [TestMethod]
        public void WhileLoopWithContinue()
        {
            DefaultTestEnvironment env = new DefaultTestEnvironment();

            var code = _typeBoxScriptEngine.Compile<DefaultTestEnvironment>(@"
Result = 0;
var i = 0;
while (i<5) {
    if (i == 3) {
        i++;
        continue;
    }
    Result += i;
    i++;
}
");

            code(env);

            Assert.AreEqual(7, env.Result);
        }

        [TestMethod]
        public void WhileLoopWithBreak()
        {
            DefaultTestEnvironment env = new DefaultTestEnvironment();

            var code = _typeBoxScriptEngine.Compile<DefaultTestEnvironment>(@"
Result = 0;
var i = 0;
while (true) {
    Result += i;
    if (Result == 10)
        break;
    i++;
}
");

            code(env);

            Assert.AreEqual(10, env.Result);
        }

        [TestMethod]
        public void DoWhileLoop()
        {
            DefaultTestEnvironment env = new DefaultTestEnvironment();

            var code = _typeBoxScriptEngine.Compile<DefaultTestEnvironment>(@"
Result = 0;
var i = 0;
do {
    Result += i;
    i++;
} while (i<5);
");

            code(env);

            Assert.AreEqual(10, env.Result);
        }

        [TestMethod]
        public void DoWhileLoopWithContinue()
        {
            DefaultTestEnvironment env = new DefaultTestEnvironment();

            var code = _typeBoxScriptEngine.Compile<DefaultTestEnvironment>(@"
Result = 0;
var i = 0;
do {
    if (i == 3) {
        i++;
        continue;
    }
    Result += i;
    i++;
} while (i<5);
");

            code(env);

            Assert.AreEqual(7, env.Result);
        }

        [TestMethod]
        public void DoWhileLoopWithBreak()
        {
            DefaultTestEnvironment env = new DefaultTestEnvironment();

            var code = _typeBoxScriptEngine.Compile<DefaultTestEnvironment>(@"
Result = 0;
var i = 0;
do {
    Result += i;
    if (Result == 10)
        break;
    i++;
} while (i<10);
");

            code(env);

            Assert.AreEqual(10, env.Result);
        }

        [TestMethod]
        public void ForOf()
        {
            DefaultTestEnvironment env = new DefaultTestEnvironment();

            env.IntEnumerable = new[] {3, 4, 5};
            env.IntList.AddRange(new[] {1, 2, 3, 4});

            Type t = env.IntEnumerable.GetType();

            var code = _typeBoxScriptEngine.Compile<DefaultTestEnvironment>(@"
Result = 0;
for (var e of IntEnumerable) {
    Result += e;
}
");

            code(env);

            Assert.AreEqual(12, env.Result);

            code = _typeBoxScriptEngine.Compile<DefaultTestEnvironment>(@"
Result = 0;
for (var e of IntList) {
    Result += e;
}
");

            code(env);

            Assert.AreEqual(10, env.Result);
        }

        [TestMethod]
        public void ForOfWithBreakContinue()
        {
            DefaultTestEnvironment env = new DefaultTestEnvironment();

            env.IntEnumerable = new[] { 1, 2, 3, 4, 5, 6 };

            Type t = env.IntEnumerable.GetType();

            var code = _typeBoxScriptEngine.Compile<DefaultTestEnvironment>(@"
Result = 0;
for (var e of IntEnumerable) {
    if (e == 3)
        continue;
    if (e == 5)
        break;
    Result++;
}
");

            code(env);

            Assert.AreEqual(3, env.Result);
        }
    }
}
