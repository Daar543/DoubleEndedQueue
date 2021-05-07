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
        protected int firstIndex = 0;
        protected int lastIndex = -1;
        protected int firstBlock = 0;
        protected int lastBlock = 0;
        private int blockCount { get => data.Length; }
        private float resizeFactor = 2;
        protected bool beingEnumerated = false;

        
        

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

            return Tuple.Create(finalBlock, finalIndex);

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
            CheckForEnumeration();
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
            CheckForEnumeration();
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
            return new DequeEnumerator(this);
            //throw new NotImplementedException();
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
            CheckForEnumeration();
            //TODO: If index is lower than half of count, push the values to the left (reduce complexity)
            this.Add(data[lastBlock][lastIndex]); //Increase the size by 1 and push the last item in there

            var(startBlock,startInd) = RecountIndex(index);
            //If moving only one block
            if(startBlock == lastBlock)
            {
                for (int j = lastIndex-1; j >= startInd; --j)
                {
                    data[startBlock][j + 1] = data[startBlock][j];
                }
                data[startBlock][startInd] = item;
                return;
            }
            //If moving more
            for(int j = lastIndex-2; j>0; --j)
            {
                data[lastBlock][j + 1] = data[lastBlock][j];
            }
            data[lastBlock][0] = data[lastBlock - 1][blockSize-1];
            for(int i = lastBlock-1; i > startBlock; --i)
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

        public T PopBack()
        {
            CheckForEnumeration();
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
            CheckForEnumeration();
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
                data[firstBlock][blockSize-1] = item;
                firstIndex = blockSize-1;
            }
            return;
        }

        public bool Remove(T item)
        {
            CheckForEnumeration();
            int idx = this.IndexOf(item);
            if (idx == -1)
                return false;
            this.RemoveAt(idx);
            return true;
        }

        public void RemoveAt(int index)
        {
            CheckForEnumeration();
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
            return new DequeEnumerator(this);
            //throw new NotImplementedException();
        }
        private class DequeEnumerator : IEnumerator<T>
        {
            public DequeEnumerator(Deque<T> dq)
            {
                
                this.dq = dq;
                dq.beingEnumerated = true;
                firstInd = dq.firstIndex;
                lastInd = dq.lastIndex;
                firstBl = dq.firstBlock;
                lastBl = dq.lastBlock;
                blocksize = dq.blockSize;
                this.i = -1;
            }
            private int i;
            private int j;
            private int blocksize;
            private int firstInd;
            private int lastInd;
            private int firstBl;
            private int lastBl;
            private Deque<T> dq;
            public T Current => getCurrent();
            private T getCurrent()
            {
                if(i == -1)
                {
                    throw new InvalidOperationException("Iteration has not started - use MoveNext()");
                }
                return dq.data[i][j];
            }
            object IEnumerator.Current => (object)this.Current; 

            public void Dispose()
            {
                dq.beingEnumerated = false;
                GC.SuppressFinalize(this);
                //throw new NotImplementedException();
            }

            public bool MoveNext()
            {
                if (firstBl > lastBl || (firstBl == lastBl && firstInd > lastInd))
                    return false;
                if (i == -1)
                {
                    i = firstBl;
                    j = firstInd;
                }
                else if (i == lastBl && j == lastInd)
                {
                    return false;
                }
                else if (j == blocksize-1)
                {
                    i += 1;
                    j = 0;
                }
                else
                {
                    j += 1;
                }
                return true;
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }
        }
    }
    public static class DequeTest
    {
        public static IList<T> GetReverseView<T>(Deque<T> d)
        {
            throw new NotImplementedException();
	    }
    }


