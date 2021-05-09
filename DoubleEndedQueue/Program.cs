﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

/// <summary>
/// The public interface methods call the private methods. The private methods (except for indexer) are independent on the reverseView,
/// while the public methods are dependent on it (and decide which private method to call), but do not accept it as argument (obviously).
/// </summary>
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
        protected int firstIndex = 0;
        protected int lastIndex = -1;
        protected int firstBlock = 0;
        protected int lastBlock = 0;
        private int blockCount { get => data.Length; }
        private float resizeFactor = 2;
        protected bool beingEnumerated = false;

        private bool reverseView = false;


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
        private void CheckForEnumeration()
        {
            if (this.beingEnumerated)
                throw new InvalidOperationException("Cannot modify enumerated collection!");
        }
        internal void InvertView()
        {
            this.reverseView ^= true;
        }

        public Deque()
        {
            data = new T[1][];
            data[0] = new T[blockSize];
        }
        private void ExpandFront()
        {
            T[][] newArr = new T[(int)((blockCount - firstBlock) * resizeFactor) + firstBlock][]; //Only the part on "right" gets resized
            for (int i = firstBlock; i <= lastBlock; ++i)
            {
                newArr[i] = data[i];
            }
            this.data = newArr;
            return;
        }
        private void ExpandBack()
        {
            T[][] newArr = new T[(int)((lastBlock + 1) * resizeFactor) + blockCount - lastBlock][]; //Only the part on "left" gets resized
            int plusSize = newArr.Length - data.Length;
            for (int i = lastBlock; i >= firstBlock; --i)
            {
                newArr[i + plusSize] = data[i];
            }
            firstBlock += plusSize;
            lastBlock += plusSize;
            this.data = newArr;
            return;
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
            for (int i = firstBlock; i <= lastBlock; ++i)
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
            if (index < 0 || index >= this.Count)
            {
                throw new ArgumentOutOfRangeException();
            }
            if (reverseView)
            {
                int addBlocks = index / blockSize;
                int pushNext = index % blockSize;
                int next = lastIndex < pushNext ? 1 : 0;
                int finalIndex = next == 1 ? blockSize + (lastIndex - pushNext) : lastIndex - pushNext;
                int finalBlock = lastBlock - addBlocks - next;

                return Tuple.Create(finalBlock, finalIndex);
            }
            else
            {

                int addBlocks = index / blockSize;
                int pushNext = index % blockSize;
                int next = (pushNext + firstIndex) / blockSize;
                int finalIndex = (pushNext + firstIndex) % blockSize;
                int finalBlock = firstBlock + addBlocks + next;

                return Tuple.Create(finalBlock, finalIndex);
            }

        }
        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= this.Count)
                {
                    throw new ArgumentOutOfRangeException();
                }
                if (reverseView)
                {
                    int addBlocks = index / blockSize;
                    int pushNext = index % blockSize;
                    int next = lastIndex < pushNext ? 1 : 0;
                    int finalIndex = next == 1 ? blockSize + (lastIndex - pushNext) : lastIndex - pushNext;
                    int finalBlock = lastBlock - addBlocks - next;

                    return data[finalBlock][finalIndex];
                }
                else
                {
                    /*
                    int addBlocks = index / blockSize;
                    int pushNext = index % blockSize;
                    int next = (pushNext + firstIndex) / blockSize;
                    int finalIndex = (pushNext + firstIndex) % blockSize;
                    int finalBlock = firstBlock + addBlocks + next;
                    */
                    return data[firstBlock + index / blockSize + (index % blockSize + firstIndex) / blockSize][(index % blockSize + firstIndex) % blockSize];
                }


            }
            set
            {
                if (index < 0 || index >= this.Count)
                {
                    throw new ArgumentOutOfRangeException();
                }
                /*
                int addBlocks = index / blockSize;
                int pushNext = index % blockSize;
                int next = (pushNext + firstIndex) / blockSize;
                int finalIndex = (pushNext + firstIndex) % blockSize;
                int finalBlock = firstBlock + addBlocks + next;
                data[finalBlock][finalIndex]=value;
                */
                if (reverseView)
                {
                    int addBlocks = index / blockSize;
                    int pushNext = index % blockSize;
                    int next = lastIndex < pushNext ? 1 : 0;
                    int finalIndex = next == 1 ? blockSize + (lastIndex - pushNext) : lastIndex - pushNext;
                    int finalBlock = lastBlock - addBlocks - next;

                    data[finalBlock][finalIndex] = value;
                    return;
                }
                else
                {
                    /*
                    int addBlocks = index / blockSize;
                    int pushNext = index % blockSize;
                    int next = (pushNext + firstIndex) / blockSize;
                    int finalIndex = (pushNext + firstIndex) % blockSize;
                    int finalBlock = firstBlock + addBlocks + next;
                    */
                    data[firstBlock + index / blockSize + (index % blockSize + firstIndex) / blockSize][(index % blockSize + firstIndex) % blockSize] = value;
                    return;
                }

            }
        }

        public int Count =>
            (lastBlock - firstBlock - 1) * blockSize //full blocks
            + blockSize - firstIndex //first block
            + lastIndex + 1; //last block

        public bool IsReadOnly => false;

        private void _Add(T item)
        {
            CheckForEnumeration();
            if (lastIndex < blockSize - 1) //Add to last block
            {
                data[lastBlock][++lastIndex] = item;
            }
            else //Create a new block
            {
                if (lastBlock + 1 == blockCount)
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
            CheckForEnumeration();
            int i = firstBlock;
            for (int j = firstIndex; j < blockSize; ++j)
            {
                this.data[i][j] = default;
            }
            for (i = firstBlock + 1; i < lastBlock; ++i)
            {
                for (int j = 0; j < blockSize; ++j)
                {
                    this.data[i][j] = default;
                }
            }
            i = lastBlock;
            for (int j = 0; j < lastIndex; ++j)
            {
                this.data[i][j] = default;
            }
            firstBlock = data.Length / 2;
            firstIndex = 0;
            data[firstBlock] = new T[blockSize];
            lastBlock = firstBlock;
            lastIndex = -1;
            return;
        }

        private bool _Contains(T item)
        {
            for (int i = Count - 1; i >= 0; --i)
            {
                if (item.Equals(this[i]))
                    return true;
            }
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array is null)
            {
                throw new ArgumentNullException();
            }
            else if (arrayIndex < 0)
            {
                throw new ArgumentOutOfRangeException();
            }
            else if (arrayIndex + this.Count >= array.Length)
            {
                throw new ArgumentException();
            }
            for (int i = 0; i < Count; ++i)
            {
                array[arrayIndex + i] = this[i];
            }
            return;
        }



        private int _IndexFromBack(T item)
        {
            int idx = 0;

            if (firstBlock == lastBlock)
            {
                for (int j = firstIndex; j <= lastIndex; ++j)
                {
                    if (Equals(item, data[firstBlock][j]))
                        return idx;
                    idx++;
                }
                return -1;
            }

            int i = firstBlock;
            for (int j = firstIndex; j < blockSize; ++j)
            {
                if (Equals(item, data[i][j]))
                    return idx;
                idx++;
            }
            for (i = firstBlock + 1; i < lastBlock; ++i)
            {
                for (int j = 0; j < blockSize; ++j)
                {
                    if (Equals(item, data[i][j]))
                        return idx;
                    idx++;
                }
            }
            i = lastBlock;
            for (int j = 0; j <= lastIndex; ++j)
            {
                if (Equals(item, data[i][j]))
                    return idx;
                idx++;
            }
            return -1;
        }
        private int _IndexFromFront(T item)
        {
            int idx = 0;

            if (lastBlock == firstBlock)
            {
                for (int j = lastIndex; j >= firstIndex; --j)
                {
                    if (Equals(item, data[lastBlock][j]))
                        return idx;
                    idx++;
                }
                return -1;
            }

            int i = lastBlock;
            for (int j = lastIndex; j >= 0; --j)
            {
                if (Equals(item, data[i][j]))
                    return idx;
                idx++;
            }
            for (i = lastBlock - 1; i > firstBlock; --i)
            {
                for (int j = blockSize - 1; j >= 0; --j)
                {
                    if (Equals(item, data[i][j]))
                        return idx;
                    idx++;
                }
            }
            i = firstBlock;
            for (int j = blockSize - 1; j >= firstIndex; --j)
            {
                if (Equals(item, data[i][j]))
                    return idx;
                idx++;
            }
            return -1;
        }

        private void _Insert(int index, T item)
        {
            CheckForEnumeration();

            //TODO: If index is lower than half of count, push the values to the left (reduce complexity)


            var (startBlock, startInd) = RecountIndex(index); //This throws exception if index not valid
            this._Add(data[lastBlock][lastIndex]);//Increase the size by 1 and push the last item in there
            //If moving only one block
            if (startBlock == lastBlock)
            {
                for (int j = lastIndex - 1; j >= startInd; --j)
                {
                    data[startBlock][j + 1] = data[startBlock][j];
                }
                data[startBlock][startInd] = item;
                return;
            }
            //If moving more
            for (int j = lastIndex - 2; j >= 0; --j)
            {
                data[lastBlock][j + 1] = data[lastBlock][j];
            }
            data[lastBlock][0] = data[lastBlock - 1][blockSize - 1];
            for (int i = lastBlock - 1; i > startBlock; --i)
            {
                for (int j = blockSize - 2; j >= 0; --j)
                {
                    data[i][j + 1] = data[i][j];
                }
                data[i][0] = data[i - 1][blockSize - 1];
            }
            for (int j = blockSize - 2; j >= startInd; --j)
            {
                data[startBlock][j + 1] = data[startBlock][j];
            }
            //Finally put the item on its position
            data[startBlock][startInd] = item;
            return;
        }

        private T _PopBack()
        {
            CheckForEnumeration();
            if (Count <= 0)
            {
                throw new InvalidOperationException();
            }
            if (firstIndex < blockSize - 1) //Still some items in this block remaining
            {
                T item = data[firstBlock][firstIndex];
                data[firstBlock][firstIndex] = default;
                firstIndex++;
                return item;
            }
            else
            {
                T item = data[firstBlock][firstIndex];
                data[firstBlock][firstIndex] = default;
                firstBlock += 1;
                firstIndex = 0;
                if (firstBlock > ((lastBlock / resizeFactor) / resizeFactor))
                {
                    ShrinkBack();
                }
                return item;
            }
        }

        private T _PopFront()
        {
            CheckForEnumeration();
            if (Count <= 0)
            {
                throw new InvalidOperationException();
            }
            if (lastIndex > 0) //Still some items in this block remaining
            {
                T item = data[lastBlock][lastIndex];
                data[lastBlock][lastIndex] = default;
                lastIndex--;
                return item;
            }
            else
            {
                T item = data[lastBlock][lastIndex];
                data[lastBlock][lastIndex] = default;
                lastBlock -= 1;
                lastIndex = blockSize - 1;
                if (lastBlock < (blockCount / resizeFactor / resizeFactor))
                {
                    ShrinkFront();
                }
                return item;
            }
        }

        private void _Prepend(T item)
        {
            CheckForEnumeration();
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
                data[firstBlock][blockSize - 1] = item;
                firstIndex = blockSize - 1;
            }
            return;
        }

        private bool _Remove(T item)
        {
            CheckForEnumeration();
            int idx = this._IndexFromBack(item);
            if (idx == -1)
                return false;
            this._RemoveAt(idx);
            return true;
        }

        private void _RemoveAt(int index)
        {

            CheckForEnumeration();
            if (index < 0 || index >= Count)
            {
                throw new ArgumentOutOfRangeException();
            }
            else
                if (index > Count / 2) //Collapse items from front
            {
                for (int i = index; i < Count - 1; ++i)
                {
                    this[i] = this[i + 1];
                }
                this.PopFront();
            }
            else //Collapse items from back
            {
                for (int i = index - 1; i >= 0; --i)
                {
                    this[i + 1] = this[i];
                }
                this.PopBack();
            }
            return;
        }
        public IEnumerator<T> GetEnumerator()
        {
            //Left enumerator goes from right to left (higher -> lower index), right enumerator is "normal" (lower -> higher index)
            if (reverseView)
                return GetLeftEnumerator();
            else
                return GetLeftEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            if (reverseView)
                return GetLeftEnumerator();
            else
                return GetLeftEnumerator();
        }

        public int IndexOf(T item)
        {
            if (reverseView)
                return _IndexFromFront(item);
            else
                return _IndexFromBack(item);
        }

        public void Insert(int index, T item)
        {
            this._Insert(index, item);
            return;
        }

        public void RemoveAt(int index)
        {
            this._RemoveAt(index);
            return;
        }

        public void Add(T item)
        {
            if (reverseView)
            {
                _Prepend(item);
            }
            else
            {
                _Add(item);
            }
        }

        public bool Contains(T item)
        {
            return (this.IndexOf(item) != -1);
        }

        public bool Remove(T item)
        {
            CheckForEnumeration();
            int idx = this.IndexOf(item);
            if (idx == -1)
                return false;
            this._RemoveAt(idx);
            return true;
        }

        public void Prepend(T item)
        {
            if (reverseView)
            {
                _Add(item);
            }
            else
            {
                _Prepend(item);
            }
        }

        public T PopBack()
        {
            if (reverseView)
            {
                return _PopFront();
            }
            else
            {
                return _PopBack();
            }
        }

        public T PopFront()
        {
            if (reverseView)
            {
                return _PopBack();
            }
            else
            {
                return _PopFront();
            }
        }
        public IEnumerator<T> GetRightEnumerator()
        {
            int i = this.firstBlock;
            int j = this.firstIndex;
                for(j = firstIndex; j < blockSize; ++j)
                {
                    yield return data[i][j];
                }
            for(i = firstBlock+1; i < lastBlock; ++i)
            {
                for(j = 0; j < blockSize; ++j)
                {
                    yield return data[i][j];
                }
            }
                for (j = 0; j <= lastIndex; ++j)
                {
                    yield return data[i][j];
                }
        }
        public IEnumerator<T> GetLeftEnumerator()
        {
            int i = this.lastBlock;
            int j = this.lastIndex;
                for (j = lastIndex; j >= 0; --j)
                {
                    yield return data[i][j];
                }
            for (i = lastBlock - 1; i > firstBlock; --i)
            {
                for (j = blockSize-1; j >= 0; --j)
                {
                    yield return data[i][j];
                }
            }
                for (j = blockSize-1; j >= firstIndex; --j)
                {
                    yield return data[i][j];
                }
        }
        
    }
    public static class DequeTest
    {
        public static IList<T> GetReverseView<T>(Deque<T> d)
        {
            d.InvertView();
            return d;
        }
    }
    class Program
    {
        public static void Test<T>(Deque<T> dq, List<T> compList)
        {
            Console.WriteLine("\n\n\n---");
            int idx = 0;
            foreach (var x in dq)
            {
                T x2 = compList[idx];
                T x3 = dq[idx];
                if (Equals(x, x2) && Equals(x, x3))
                    Console.WriteLine(x);
                else
                {
                    if (!Equals(x, x3))
                        Console.WriteLine($"Difference in foreach");
                    Console.WriteLine($"Difference at index {idx}: {x} vs {x2}");
                }
                idx += 1;
            }
        }
        public static void Invert<T>(ref List<T> lst)
        {
            var invlist = new List<T>(lst.Count);
            for (int i = 0; i < lst.Count; ++i)
            {
                invlist.Add(lst[lst.Count - 1 - i]);
            }
            lst = invlist;
            return;
        }
        public static void Main(string[] args)
        {
            Deque<int> DQ = new Deque<int>();
            var DQ2 = new Deque<string>();


            DQ2.Add("jůůů");
            DQ2.Add("hele");
            DQ2.Clear();
            DQ2.Remove("jůůů");
            DQ2.Add(null);
            DQ2.Add("ll");
            /*DQ.Add(7);
            DQ.Add(15);
            DQ.Add(42);
            DQ.Insert(1, 44);
            Console.WriteLine(DQ.IndexOf(42));

            DQ.RemoveAt(1);
            DQ.Remove(15);
            DQ.Remove(42);

            DQ.Prepend(22);
            
            foreach(var x in DQ)
            {
                Console.WriteLine(x);
            }*/
            foreach (var x in DQ2)
            {
                Console.WriteLine(x);
            }
            foreach (var x in DQ)
            {
                Console.WriteLine(x);
            }



            Random r = new Random();
            r.Next(1, 100);

            List<int> bmlist = new List<int>();

            DQ.Add(0);
            DQ.Add(1);
            DQ.Add(2);
            DQ.Add(3);

            DQ.Insert(1, 6);
            DQ.Remove(6);
            DequeTest.GetReverseView(DQ);
            DQ.Insert(1, 6);
            DQ.Remove(6);

            DQ = new Deque<int>();
            int maxlen = 2000;


            Test(DQ, bmlist);
            for (int i = 0; i < 1000; ++i)
            {
                int randint = r.Next(0, 100);
                DQ.Add(randint);
                bmlist.Add(randint);
            }
            Test(DQ, bmlist);
            for (int i = 0; i < 100; ++i)
            {
                int idx = r.Next(-bmlist.Count, 2 * bmlist.Count);
                try
                {
                    Console.WriteLine(DQ[idx]);

                }
                catch (ArgumentOutOfRangeException)
                {
                    Console.WriteLine("-1");
                }
                try
                {
                    Console.WriteLine(bmlist[idx]);

                }
                catch (ArgumentOutOfRangeException)
                {
                    Console.WriteLine("-1");
                }
            }
            Test(DQ, bmlist);
            for (int i = maxlen; i >= maxlen / 4.3; --i)
            {
                int randint = r.Next(0, 100);
                DQ.Remove(randint);
                bmlist.Remove(randint);
            }
            Test(DQ, bmlist);
            int cnt = DQ.Count;
            for (int i = 0; i < maxlen / 1.6; ++i)
            {
                int randint = r.Next(0, 100);

                int randidx = r.Next(0, cnt);
                DQ[randidx] = randint;
                bmlist[randidx] = randint;
            }
            DQ.InvertView();
            Invert(ref bmlist);
            Test(DQ, bmlist);


            for (int i = maxlen; i >= maxlen / 4.3; --i)
            {
                int randint = r.Next(0, 10000);
                DQ.Insert(0, randint);
                bmlist.Insert(0, randint);
            }
            Console.WriteLine(DQ[^2]);
            Test(DQ, bmlist);


            for (int i = 0; i < maxlen / 10; ++i)
            {
                int randidx = r.Next(0, DQ.Count);
                DQ.RemoveAt(randidx);
                bmlist.RemoveAt(randidx);
            }
            Test(DQ, bmlist);
            for (int i = maxlen; i >= maxlen / 4.3; --i)
            {
                int randint = r.Next(0, 10000);
                DQ.Insert(1, randint);
                bmlist.Insert(1, randint);
            }
            Test(DQ, bmlist);
            for (int i = 0; i < DQ.Count; ++i)
            {
                int randint = r.Next(0, 10000);
                DQ.RemoveAt(0);
                bmlist.RemoveAt(0);
            }
            Test(DQ, bmlist);
            bmlist.Clear();
            DQ.Clear();
            Console.WriteLine("---\n{0}", bmlist.Count);
            Console.WriteLine("---\n{0}", DQ.Count);
            Test(DQ, bmlist);
            for (int i = 0; i < DQ.Count; ++i)
            {
                int randint = r.Next(0, 10000);
                DQ.RemoveAt(0);
                bmlist.RemoveAt(0);
            }


            DQ.Add(1);
            DQ.IndexOf(5);
            DQ.RemoveAt(0);
            DQ.Remove(7);
            DQ.Insert(5, 0);
        }
    }
}

