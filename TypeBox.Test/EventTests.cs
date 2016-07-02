using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TypeBox.Test
{
    [TestClass]
    public class EventTests
    {
        private class EventTestEnvironment
        {
            public int Result = 0;

            public event Func<string, object> Event;

            public event Action<string> Event2;

            public event Action<string, int, EventTestEnvironment, Dictionary<string, string>> Event3;

            public virtual void OnEvent3()
            {
                Action<string, int, EventTestEnvironment, Dictionary<string, string>> handler = Event3;
                if (handler != null)
                {
                    handler("string", 123, this, new Dictionary<string, string>());
                }
            }

            public virtual void OnEvent2()
            {
                Action<string> handler = Event2;
                if (handler != null)
                {
                    handler("hello2");
                }
            }

            public virtual void OnEvent()
            {
                var handler = Event;
                if (handler != null) handler("hello");
            }
        }

        private TypeBoxScriptEngine _typeBoxScriptEngine;

        [TestInitialize]
        public void Initialize()
        {
            _typeBoxScriptEngine = new TypeBoxScriptEngine();
        }

        [TestMethod]
        public void HandleEventLambda()
        {
            var environment = new EventTestEnvironment();

            _typeBoxScriptEngine.Execute(@"
Event += (str : string) : object => { Result = 33; };
", environment);

            Assert.AreEqual(0, environment.Result);
            environment.OnEvent();
            Assert.AreEqual(33, environment.Result);
        }

        [TestMethod]
        public void HandleEvent()
        {
            var environment = new EventTestEnvironment();

            _typeBoxScriptEngine.Execute(@"
function HandleEvent(str : string) : object {
    Result = 33;
    return null;
}

Event += HandleEvent;
", environment);

            Assert.AreEqual(0, environment.Result);
            environment.OnEvent();
            Assert.AreEqual(33, environment.Result);
        }

        [TestMethod]
        public void ShallRemoveEventHandler()
        {
            var environment = new EventTestEnvironment();

            _typeBoxScriptEngine.Execute(@"
function HandleEvent(str : string) : object {
    Result = 33;
    return null;
}

Event += HandleEvent;
Event -= HandleEvent;
            ", environment);
            
            Assert.AreEqual(0, environment.Result);
            environment.OnEvent();
            Assert.AreEqual(0, environment.Result);
        }
    }
}
