namespace TypeBox
{
    class TypeBoxCompileMessage
    {
        private readonly int _lineNumber;
        private readonly int _charNumber;
        private readonly string _message;

        public TypeBoxCompileMessage(int lineNumber, int charNumber, string message)
        {
            _lineNumber = lineNumber;
            _charNumber = charNumber;
            _message = message;
        }

        public int LineNumber
        {
            get { return _lineNumber; }
        }

        public int CharNumber
        {
            get { return _charNumber; }
        }

        public string Message
        {
            get { return _message; }
        }
    }
}
