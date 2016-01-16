
namespace TypeBox.Test
{
    class SubEnvironment
    {
        public int Kalle;

        // Should not be accessable from script
        private int _noAccess = 0;

        public void SetKalle(int val)
        {
            Kalle = val;
        }
    }

    class DefaultTestEnvironment
    {
        public int IntVar;
        public float FloatVar;
        public bool BoolVar;

        public int Result;

        public SubEnvironment SubEnv = new SubEnvironment();
    }
}
