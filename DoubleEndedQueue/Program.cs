﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace DoubleEndedQueue
{
    interface IDeque<T>
    {
        public void Prepend(T item);
        public void Add(T item);
        public T PopBack();
        public T PopFront();
    }
    public class Deque<T> : IDeque<T>, IList<T>
    {
        private readonly int blockSize = 32;
        private int firstIndex = 0;
        private int lastIndex = -1;
        private int firstBlock = 0;
        private int lastBlock = 0;
        private int blockCount = 1;
        private float resizeFactor = 2;

        private T[][] data;

        public Deque()
        {
            data = new T[1][];
            data[0] = new T[blockSize];
        }
        private int getBlock(int index)
        {
            if (index < 0)
            {
                throw new IndexOutOfRangeException();
            }
            int remaining = blockSize - firstIndex;
            int addBlocks = index / blockSize;
            int pushNext = index % blockSize;
            int next = (pushNext + firstIndex) / blockSize;
            int finalIndex = (pushNext + firstIndex) % blockSize;

            return firstBlock + addBlocks + next;
        }
        private void Expand()
        {
            throw new NotImplementedException();
        }
        private void Shrink()
        {
            throw new NotImplementedException();
        }
        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= this.Count)
                {
                    throw new IndexOutOfRangeException();
                }
                /*
                int addBlocks = index / blockSize;
                int pushNext = index % blockSize;
                int next = (pushNext + firstIndex) / blockSize;
                int finalIndex = (pushNext + firstIndex) % blockSize;
                int finalBlock = firstBlock + addBlocks + next;
                */

                return data[firstBlock + index / blockSize + (index % blockSize + firstIndex) / blockSize][(index % blockSize + firstIndex) % blockSize];
            }
            set
            {
                if (index < 0 || index >= this.Count)
                {
                    throw new IndexOutOfRangeException();
                }
                /*
                int addBlocks = index / blockSize;
                int pushNext = index % blockSize;
                int next = (pushNext + firstIndex) / blockSize;
                int finalIndex = (pushNext + firstIndex) % blockSize;
                int finalBlock = firstBlock + addBlocks + next;
                data[finalBlock][finalIndex]=value;
                */

                data[firstBlock + index / blockSize + (index % blockSize + firstIndex) / blockSize][(index % blockSize + firstIndex) % blockSize] = value;
            }
        }

        public int Count => (lastBlock - firstBlock - 1) * blockSize + blockSize - firstIndex + lastIndex + 1; //throw new NotImplementedException();

        public bool IsReadOnly => false; //throw new NotImplementedException();

        public void Add(T item)
        {
            if (lastIndex < blockSize - 1) //Add to last block
            {
                data[lastBlock][++lastIndex] = item;
            }
            else //Create a new block
            {
                lastBlock += 1;
                if (lastBlock == blockCount)
                {
                    Expand();
                }
                data[lastBlock][0] = item;
                lastIndex = 0;
            }
            return;
            
        }

        public void Clear()
        {
            for (int i = 0; i < Count; ++i)
            {
                this[i] = default;
            }
            return;
            throw new NotImplementedException();
        }

        public bool Contains(T item)
        {
            for(int i = Count-1; i>=0; --i)
            {
                if (item.Equals(this[i]))
                    return true;
            }
            return false;
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if(array is null)
            {
                throw new ArgumentNullException();
            }
            else if(arrayIndex < 0)
            {
                throw new ArgumentOutOfRangeException();
            }
            else if(arrayIndex+this.Count >= array.Length)
            {
                throw new ArgumentException();
            }
            for(int i = 0; i < Count; ++i)
            {
                array[arrayIndex + i] = this[i];
            }
            return;

            throw new NotImplementedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public int IndexOf(T item)
        {
            for(int i = 0; i < this.Count; ++i)
            {
                if (item.Equals(this[i]))
                    return i;
            }
            return -1;
            throw new NotImplementedException();
        }

        public void Insert(int index, T item)
        {
            this.Add(this[this.Count - 1]);
            for(int i = this.Count-2; i > index; --i)
            {
                this[i] = this[i - 1];
            }
            this[index] = item;
            return;
        }

        public T PopBack()
        {
            if (Count <= 0)
            {
                throw new InvalidOperationException();
            }
            if (firstIndex < blockSize - 1) //Still some items in this block remaining
            {
                return data[firstBlock][firstIndex++];
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public T PopFront()
        {
            if (Count <= 0)
            {
                throw new InvalidOperationException();
            }
            if (lastIndex > 0) //Still some items in this block remaining
            {
                return data[lastBlock][lastIndex--];
            }
            else
            {
                T item = data[lastBlock][lastIndex];
                lastBlock -= 1;
                lastIndex = blockSize;
                if (lastBlock < (blockCount / resizeFactor / resizeFactor))
                {
                    Shrink();
                }
                return item;
            }
        }

        public void Prepend(T item)
        {
            if (firstIndex > 0) //Add to first block
            {
                data[firstBlock][--firstIndex] = item;
            }
            else //Create a new block
            {
                throw new NotImplementedException();
            }
            return;
        }

        public bool Remove(T item)
        {
            int idx = this.IndexOf(item);
            if (idx == -1)
                return false;
            this.RemoveAt(idx);
            return true;

            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            if (index < 0)
            {
                throw new IndexOutOfRangeException();
            }
            else if(index > Count / 2) //Collapse items from front
            {
                for(int i = Count-1; i > index; --i)
                {
                    this[i] = this[i + 1];
                }
                this.PopFront();
            }
            else //Collapse items from back
            {
                for (int i = 0; i < index; ++i)
                {
                    this[i+1] = this[i];
                }
                this.PopBack();
            }
            return;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
    public static class DequeTest
    {
        public static IList<T> GetReverseView<T>(Deque<T> d)
        {
            throw new NotImplementedException();
	    }
    }
    class Program
    {
        public static void Main(string[] args)
        {
            Deque<int> DQ = new Deque<int>();

            DQ.Add(7);
            DQ.Add(15);
            DQ.Add(42);

            DQ.RemoveAt(1);
            DQ.Remove(15);
            DQ.Remove(42);

            DQ.Prepend(22);



            /*Random r = new Random();
            r.Next(1, 100);

            List<int> bmlist = new List<int>();

            int maxlen = 10000;
            for(int i = 0; i < maxlen; ++i)
            {
                int randint = r.Next(0, 1000);
                bmlist.Add(randint);
                DQ.Add(randint);
            }

            int[] bmarray = new int[] { 7, 15, 42 };
            bmarray = bmlist.ToArray();
            int repetitions = 100000;
            long accum = 0;
            Stopwatch sw = new Stopwatch();

            sw.Start();
            for(int rep = 0; rep < repetitions; ++rep)
            {
                for(int i = 0; i < bmarray.Length; ++i)
                {
                    accum += bmarray[i];
                }
                accum = 0;
            }
            sw.Stop();
            var e1 = sw.Elapsed;

            sw = new Stopwatch();
            sw.Start();
            for (int rep = 0; rep < repetitions; ++rep)
            {
                for (int i = 0; i < DQ.Count; ++i)
                {
                    accum += DQ[i];
                }
                accum = 0;
            }
            sw.Stop();
            var e2 = sw.Elapsed;

            Console.WriteLine(e1);
            Console.WriteLine(e2);*/
        }
    }
}
