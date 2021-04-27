using System;
using System.Collections;
using System.Collections.Generic;

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
        private readonly int dataBlockSize = 32;
        private int firstIndex;
        private int lastIndex;
        private int firstBlock;
        private int lastBlock;

        private T[][] data;

        private int getBlock(int index)
        {
            if (index < 0)
            {
                throw new IndexOutOfRangeException();
            }
            int remaining = dataBlockSize - firstIndex;
            int addBlocks = index / dataBlockSize;
            int pushNext = index % dataBlockSize;
            int next = (pushNext + firstIndex) / dataBlockSize;
            int finalIndex = (pushNext + firstIndex) % dataBlockSize;

            return firstBlock + addBlocks + next;
        }

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= this.Count)
                {
                    throw new IndexOutOfRangeException();
                }
                int addBlocks = index / dataBlockSize;
                int pushNext = index % dataBlockSize;
                int next = (pushNext + firstIndex) / dataBlockSize;
                int finalIndex = (pushNext + firstIndex) % dataBlockSize;
                int finalBlock = firstBlock + addBlocks + next;

                return data[finalBlock][finalIndex];
            }
            set
            {
                if (index < 0 || index >= this.Count)
                {
                    throw new IndexOutOfRangeException();
                }
                int addBlocks = index / dataBlockSize;
                int pushNext = index % dataBlockSize;
                int next = (pushNext + firstIndex) / dataBlockSize;
                int finalIndex = (pushNext + firstIndex) % dataBlockSize;
                int finalBlock = firstBlock + addBlocks + next;

                data[finalBlock][finalIndex] = value;
            }
        }

        public int Count => (lastBlock - firstBlock - 1) * dataBlockSize + dataBlockSize - firstIndex + lastIndex + 1; //throw new NotImplementedException();

        public bool IsReadOnly => false; //throw new NotImplementedException();

        public void Add(T item)
        {
            if (lastIndex < dataBlockSize - 1) //Add to last block
            {
                data[lastBlock][++lastIndex] = item;
            }
            else //Create a new block
            {

            }
            throw new NotImplementedException();
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
            //Not as effective, try rewriting
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
            if (firstIndex < dataBlockSize - 1) //Still some items in this block remaining
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
                throw new NotImplementedException();
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

            }
            throw new NotImplementedException();
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
            else if(index > Count / 2) //Collapse items from back
            {
                for(int i = Count-1; i > index; --i)
                {
                    this[i] = this[i + 1];
                }
                this[Count - 1] = default;
                if(this.lastIndex == 0)
                {
                    this.lastIndex = dataBlockSize;
                    --lastBlock;
                }
                else
                {
                    this.lastIndex--;
                }
            }
            else //Collapse items from front
            {
                for (int i = 0; i < index; ++i)
                {
                    this[i+1] = this[i];
                }
                this[0] = default;
                if (this.firstIndex == dataBlockSize)
                {
                    this.firstIndex = 0;
                    ++this.firstBlock;
                }
                else
                {
                    this.firstIndex++;
                }
            }
            return;
            throw new NotImplementedException();
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

        }
    }
}
