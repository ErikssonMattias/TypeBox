﻿using System;
using System.Collections.Generic;

namespace TypeBox
{
    class TypeBoxParseException : Exception
    {
        private readonly IEnumerable<TypeBoxCompileMessage> _messages;
        private readonly Exception _innerException;

        public TypeBoxParseException(IEnumerable<TypeBoxCompileMessage> messages)
        {
            _messages = messages;
            _innerException = null;
        }

        public TypeBoxParseException(Exception innerException)
        {
            _messages = null;
            _innerException = innerException;
        }

        public IEnumerable<TypeBoxCompileMessage> Messages
        {
            get { return _messages; }
        }

        public override string Message
        {
            get
            {
                if (_messages == null)
                {
                    return _innerException.Message;
                }

                return string.Join(", ", _messages);
            }
        }

        public override string ToString()
        {
            if (_messages == null)
            {
                return _innerException.ToString();
            }

            return string.Join(", ", _messages);
        }
    }

    class LucCompileException : Exception
    {
        public LucCompileException()
        {
        }

        public LucCompileException(string message) : base(message)
        {
        }

        public LucCompileException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
