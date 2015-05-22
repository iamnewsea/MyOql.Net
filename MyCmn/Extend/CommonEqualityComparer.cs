using System;
using System.Collections.Generic;

namespace MyCmn
{
    public class CommonEqualityComparer<T> : IEqualityComparer<T>
    {
        // Fields
        private Func<T, T, bool> comp;

        // Methods
        public CommonEqualityComparer(Func<T, T, bool> Comparer)
        {
            this.comp = Comparer;
        }

        public bool Equals(T x, T y)
        {
            if (null != this.comp)
            {
                return this.comp(x, y);
            }
            return object.Equals(x, y);
        }

        public int GetHashCode(T obj)
        {
            if (object.Equals(obj, null))
            {
                return 0;
            }
            return obj.ToString().GetHashCode();
        }
    }



}
