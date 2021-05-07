﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace DoubleEndedQueue
{
    class Enumerator<T> : IEnumerator<T>
    {
        public T Current => throw new NotImplementedException();

        object IEnumerator.Current => throw new NotImplementedException();

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool MoveNext()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }
    
    
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
        private int blockCount { get => data.Length; }
        private float resizeFactor = 2;

        private T[][] data;
        /*public void Iterate(Action<T> clearance)
        {
            int i = firstBlock;
            for (int j = firstIndex; j < blockSize; ++j)
            {
                clearance(data[i][j]);
            }
            for (i = firstBlock + 1; i < lastBlock; ++i)
            {
                for (int j = 0; j < blockSize; ++j)
                {
                    clearance(data[i][j]);
                }
            }
            i = lastBlock;
            for (int j = 0; j < lastIndex; ++j)
            {
                clearance(data[i][j]);
            }
            return;
        }*/

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
        private void ExpandFront()
        {
            T[][] newArr = new T[(int)((blockCount - firstBlock) * resizeFactor)+firstBlock][]; //Only the part on "right" gets resized
            for(int i = firstBlock; i<=lastBlock; ++i)
            {
                newArr[i] = data[i];
            }
            this.data = newArr;
            return;
        }
        private void ExpandBack()
        {
            T[][] newArr = new T[(int)((lastBlock+1) * resizeFactor) + blockCount-lastBlock][]; //Only the part on "left" gets resized
            int plusSize = newArr.Length - data.Length;
            for (int i = lastBlock; i >= firstBlock; --i)
            {
                newArr[i+plusSize] = data[i];
            }
            firstBlock += plusSize;
            lastBlock += plusSize;
            this.data = newArr;
            return;
            throw new NotImplementedException();
        }
        private void ShrinkFront()
        {
            T[][] newArr = new T[(int)((blockCount - firstBlock) / resizeFactor) + firstBlock][]; //Only the part on "right" gets resized
            for (int i = firstBlock; i <= lastBlock; ++i)
            {
                newArr[i] = data[i];
            }
            this.data = newArr;
            return;
        }
        private void ShrinkBack()
        {
            T[][] newArr = new T[blockCount - (int)(firstBlock / resizeFactor)][];
            int minusSize = data.Length - newArr.Length;
            for(int i = firstBlock; i <= lastBlock; ++i)
            {
                newArr[i - minusSize] = data[i];
            }
            firstBlock -= minusSize;
            lastBlock -= minusSize;
            this.data = newArr;
            return;
        }
        private Tuple<int, int> RecountIndex(int index)
        {

            int addBlocks = index / blockSize;
            int pushNext = index % blockSize;
            int next = (pushNext + firstIndex) / blockSize;
            int finalIndex = (pushNext + firstIndex) % blockSize;
            int finalBlock = firstBlock + addBlocks + next;

            return Tuple.Create(finalIndex, finalBlock);

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

        public int Count => 
            (lastBlock - firstBlock - 1) * blockSize //full blocks
            + blockSize - firstIndex //first block
            + lastIndex + 1; //last block

        public bool IsReadOnly => false;

        public void Add(T item)
        {
            if (lastIndex < blockSize - 1) //Add to last block
            {
                data[lastBlock][++lastIndex] = item;
            }
            else //Create a new block
            {
                if (lastBlock+1 == blockCount)
                {
                    ExpandFront();
                }
                data[++lastBlock] = new T[blockSize];
                data[lastBlock][0] = item;
                lastIndex = 0;
            }
            return;
            
        }

        public void Clear()
        {
            int i = firstBlock;
            for (int j = firstIndex; j < blockSize; ++j)
            {
                this.data[i][j] = default;
            }
            for (i = firstBlock+1; i < lastBlock; ++i)
            {
                for(int j = 0; j < blockSize; ++j)
                {
                    this.data[i][j] = default;
                }
            }
            i = lastBlock;
            for (int j = 0; j < lastIndex; ++j)
            {
                this.data[i][j] = default;
            }
            return;
        }

        public bool Contains(T item)
        {
            for(int i = Count-1; i>=0; --i)
            {
                if (item.Equals(this[i]))
                    return true;
            }
            return false;
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
            int idx = 0;
            int i = firstBlock;
            for (int j = firstIndex; j < blockSize; ++j)
            {
                if (item.Equals(data[i][j]))
                    return idx;
                idx++;
            }
            for (i = firstBlock + 1; i < lastBlock; ++i)
            {
                for (int j = 0; j < blockSize; ++j)
                {
                    if (item.Equals(data[i][j]))
                        return idx;
                    idx++;
                }
            }
            i = lastBlock;
            for (int j = 0; j < lastIndex; ++j)
            {
                if (item.Equals(data[i][j]))
                    return idx;
                idx++;
            }
            return -1;
        }

        public void Insert(int index, T item)
        {
            //TODO: If index is lower than half of count, push the values to the left (reduce complexity)
            this.Add(data[lastBlock][lastIndex]); //Increase the size by 1 and push the last item in there

            var(startBlock,startInd) = RecountIndex(index);

            for(int j = startInd; j < blockSize - 1; ++j)
            {
                data[startBlock][j + 1] = data[startBlock][j];
            }
            data[startBlock + 1][0] = data[startBlock][blockSize - 1];
            for(int i = startBlock+1; i < lastBlock; ++i)
            {
                for (int j = 0; j < blockSize - 1; ++j)
                {
                    data[i][j + 1] = data[i][j];
                }
                data[i + 1][0] = data[i][blockSize - 1];
            }
            for(int j = 0; j < lastIndex - 1; ++j)
            {
                data[lastBlock][j + 1] = data[lastBlock][j];
            }
            
            data[startBlock][startInd] = item;
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
                T item = data[firstBlock][firstIndex];
                firstBlock += 1;
                firstIndex = 0;
                if (firstBlock > (lastBlock / resizeFactor / resizeFactor))
                {
                    ShrinkBack();
                }
                return item;
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
                lastIndex = blockSize-1;
                if (lastBlock < (blockCount / resizeFactor / resizeFactor))
                {
                    ShrinkFront();
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
                if (firstBlock - 1 < 0)
                {
                    ExpandBack();
                }
                data[--firstBlock] = new T[blockSize];
                data[firstBlock][blockSize-1] = item;
                firstIndex = blockSize-1;
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
    /**/class Program
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



            Random r = new Random();
            r.Next(1, 100);

            List<int> bmlist = new List<int>();

            int maxlen = 10000;
            for(int i = 0; i < maxlen; ++i)
            {
                int randint = r.Next(0, 10000);
                bmlist.Add(randint);
                DQ.Add(randint);
            }
            for(int i = maxlen; i >= maxlen/4.3; --i)
            {
                DQ.PopFront();
            }
            for (int i = 0; i < maxlen / 1.4; ++i)
            {
                int randint = r.Next(0, 10000);
                DQ.Prepend(randint);
            }
            for (int i = 0; i < maxlen / 2.2; ++i)
            {
                DQ.PopBack();
            }
            int[] bmarray = new int[] { 7, 15, 42 };
            bmarray = bmlist.ToArray();

            for (int i = 0; i < bmarray.Length; ++i)
            {
                if (bmarray[i] != DQ[i+2])
                    throw new Exception($"Mistake at index {i}");
            }
            Console.WriteLine("Collections are equal");


            int repetitions = 1000;
            long accum = 0;
            Stopwatch sw = new Stopwatch();

            sw = new Stopwatch();
            sw.Start();
            for (int rep = 0; rep < repetitions; ++rep)
            {
                DQ.Clear();
            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);

            sw = new Stopwatch();
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
            Console.WriteLine(e2);
        }
    }/**/
}
