
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace TypeBox.Test
{
#pragma warning disable 0169
#pragma warning disable 0649

    internal class SubSubEnvironment
    {

    }

    internal class SubEnvironment
    {
        public int Kalle;

        public SubSubEnvironment SubEnv = new SubSubEnvironment();

        // Should not be accessable from script
        private int _noAccess = 0;

        public void SetKalle(int val)
        {
            Kalle = val;
        }
    }

    internal class DefaultTestEnvironment
    {
        public int IntVar;
        public float FloatVar;
        public double DoubleVar;
        public bool BoolVar;

        public int Result;

        public IEnumerable<int> IntEnumerable;
        public int[] IntArray = new int[0];
        public double[] DoubleArray = new double[0];

        public List<int> IntList = new List<int>(); 

        public SubEnvironment SubEnv = new SubEnvironment();

        public void DoNothing()
        {
            Debug.WriteLine("Nothing!");
        }

        public class EnvironmentClass
        {
            public int ClassInt;
        }

        public EnvironmentClass ClassInstance = new EnvironmentClass();

        public object ObjectInstance;

        public Func<int, int> FuncIntInIntOut;

        public event Action<int> EventWithInt;

        public void FireEventWithInt(int i)
        {
            EventWithInt?.Invoke(i);
        }
    }
#pragma warning restore 0169
#pragma warning restore 0649
}
