using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;

namespace TypeBox.Run
{
    class Olle
    {
        public IEnumerable<int> IntList;

        public int PublicInt = 34;

        public int Func(int hej)
        {
            return hej + 7;
        }
    }

    class Program
    {
        public delegate void MyDeligate(int n);

        public static event Action<int> Events;

        public static event Func<int, int> PublicEvent;

        static void Main(string[] args)
        {
            // Playground
            IEnumerable<float> hej = new List<float>();

            Type lt = hej.GetType();

            if (lt == typeof (IEnumerable<>))
            {
                Console.WriteLine("Generic enumerable");
            }
            else if (lt == typeof(IEnumerable<float>))
            {
                Console.WriteLine("Specific enumerable");
            }
            else if (lt.GetInterfaces().Contains(typeof (IEnumerable<>)))
            {
                Console.WriteLine("contains generic enumerable");
            }
            else if (lt.GetInterfaces().Contains(typeof(IEnumerable<float>)))
            {
                Console.WriteLine("contains specific enumerable");
            }

            Console.WriteLine(lt);

            object nisse = 3;

            Console.WriteLine(nisse.GetType());

            //MyDeligate myDel;

            //Events += par => { Console.WriteLine(par);};

            //var returnLabel = Expression.Label(typeof(int));

            //ParameterExpression param = Expression.Parameter(typeof (int));
            

            //var lam =
            //    Expression.Lambda<Func<int, int>>(
            //        Expression.Block(new Expression[]
            //        {
            //            Expression.Return(returnLabel, Expression.Add(param, Expression.Constant(3))),
            //            Expression.Empty(),
            //            Expression.Label(returnLabel)
            //        }),
            //        param).Compile();

            //Console.WriteLine(lam(5));

            //var del = new Action<int>(Expression.Lambda(Expression.Constant(null), "test", ));

            Func<int, int> func = i => i;

            PublicEvent += func;

            if (PublicEvent != null)
            {
                var bertil = PublicEvent(4);
            }
            
            var olle = new Olle();
            if ((new Random()).Next() > 10)
            {
                olle = null;
            }

            olle = null;

            olle?.Func(8);

            double d = 4.5;
            float f = 7.8f;
            int ii = 2;

            d /= f;
            d /= ii;
            d += ii;

            d = ii;

            d = f;
            Console.WriteLine(2*d);

            IList<Type> l = new List<Type>();
            var gw = typeof (List<>);
            
        }
    }
}
