using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

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
            for (int j = 0; j < blockSize; ++j)
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
            for (int j = 0; j < blockSize; ++j)
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