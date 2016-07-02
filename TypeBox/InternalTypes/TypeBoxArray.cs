using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeBox.InternalTypes
{
    class TypeBoxArray<T> : List<T>
    {
        public TypeBoxArray()
        {
        }

        public TypeBoxArray(int capacity) : base(capacity)
        {
        }

        public TypeBoxArray(IEnumerable<T> collection) : base(collection)
        {
        }

        public void push(T item)
        {
            Push(item);
        }

        public void Push(T item)
        {
            Add(item);
        }

        public Type ElementType
        {
            get { return GetType().GetGenericArguments()[0]; }
        }

        public IEnumerable<T> CastToIEnumerable<T>()
        {
            return this.Cast<T>();
        }

        public TCast[] CastToArray<TCast>()
        {
            return this.Cast<TCast>().ToArray();
        }

        public TCast[] ChangeTypeOfMembersToArray<TCast>()
        {
            // Very nice!
            return this.Select(x => (TCast)Convert.ChangeType(x, typeof(TCast))).ToArray();
        }
    }
}
