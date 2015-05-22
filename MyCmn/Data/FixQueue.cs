// 
//  FixQueue.cs
//  
//  Author:
//       newsea <iamnewsea@yahoo.com.cn>
// 
//  Copyright (c) 2010 newsea

using System;
using System.Collections.Generic;

namespace MyCmn
{

    /// <summary>
    /// 指定数量的 队列， 达到最大数量后，队首自动弹出。
    /// </summary>
    public class FixQueue<T> : Queue<T>
    {
        //public delegate void DequeueDelegate(T DequeuedObject, bool AsFix);
        public event Action<T, bool> OnDequeue;

        protected int FixLength = 100;
        public FixQueue(int Length)
        {
            FixLength = Length;
        }

        public FixQueue()
            : this(100)
        {
        }

        /// <summary>
        /// 出列。 
        /// </summary>
        /// <returns></returns>
        public new T Dequeue()
        {
            T dequeobj = base.Dequeue();
            if (OnDequeue != null)
            {
                OnDequeue(dequeobj, false);
            }
            return dequeobj;
        }

        /// <summary>
        /// 将对象添加到 System.Collections.Generic.FixQueue 的结尾处。
        /// </summary>
        /// <param name="obj"></param>
        public new void Enqueue(T obj)
        {
            if (this.Count >= FixLength)
            {
                T dequeobj = base.Dequeue();
                if (OnDequeue != null)
                {
                    OnDequeue(dequeobj, true);
                }
            }
            base.Enqueue(obj);
        }
    }
}
