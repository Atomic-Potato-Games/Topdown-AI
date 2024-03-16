using System;

namespace Pathfinding
{
    public interface IHeapItem<T> : IComparable<T>
    {
        /// <summary>
        /// Helps keep track of the items index in the heap tree
        /// </summary>
        int HeapIndex
        {
            get;
            set;
        }
    }

    // Note: The CompareTo method returns a priority
    //       higher priority = 1
    //       equal priority = 0   
    //       lower priority = -1

    /// <summary>
    /// A list tree optimized for searching (More info: https://youtu.be/3Dw5d7PlcTM?si=4AJpaMgkWFhsVYnT)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Heap<T> where T : IHeapItem<T>
    {
        /// <summary>
        /// The elements of the heap tree
        /// </summary>
        T[] _items;
        int _itemsCount;
        /// <summary>
        /// The total number of elements in the heap tree
        /// </summary>
        public int Count => _itemsCount;

        public Heap(int maxSize)
        {
            _items = new T[maxSize];
        }

        /// <summary>
        /// Checks if the heap tree contains the item
        /// </summary>
        /// <param name="item">The item to be found</param>
        public bool Contains(T item)
        {
            if (item.HeapIndex < _itemsCount)
                return Equals(_items[item.HeapIndex], item);
            else
                return false;
        }

        /// <summary>
        /// Empties the heap tree
        /// </summary>
        public void Clear()
        {
            _itemsCount = 0;
        }

        /// <summary>
        /// Adds an item to the heap tree
        /// </summary>
        public void Add(T item)
        {
            // Add the item to the end of the list
            item.HeapIndex = _itemsCount;
            _items[_itemsCount] = item;
            
            SortUp(item);
            _itemsCount++;
        }

        /// <summary>
        /// Returns the top element of the heap tree / the root telement
        /// </summary>
        public T RemoveFirst()
        {
            T firstItem = _items[0];
            _itemsCount--;
            _items[0] = _items[_itemsCount];
            _items[0].HeapIndex = 0;
            SortDown(_items[0]);
            return firstItem;
        }

        /// <summary>
        /// Updates the item's position in the tree
        /// </summary>
        public void UpdateItem(T item)
        {
            SortUp(item);
        }

        /// <summary>
        /// Swaps item with its parents until it reaches a parent with a lower priority
        /// </summary>
        /// <param name="item">The item to move up the heap</param>
        void SortUp(T item)
        {
            int parentIndex = (item.HeapIndex-1)/2;

            while(true)
            {
                T parentItem = _items[parentIndex];
                if (item.CompareTo(parentItem) > 0)
                    Swap(item, parentItem);
                else
                    break;
                parentIndex = (item.HeapIndex-1)/2;
            }
        }

        /// <summary>
        /// Swaps item with its children until it reaches children with higher priority
        /// </summary>
        /// <param name="item">The item to move down the heap</param>
        void SortDown(T item)
        {
            while (true)
            {
                int childItemToSwapIndex;
                int leftChildIndex = item.HeapIndex * 2 + 1;
                int rightChildIndex = item.HeapIndex * 2 + 2;

                if (isItemExists(leftChildIndex))
                {
                    childItemToSwapIndex = leftChildIndex;

                    if (isItemExists(rightChildIndex))
                    {
                        if (_items[leftChildIndex].CompareTo(_items[rightChildIndex]) < 0)
                            childItemToSwapIndex = rightChildIndex;
                    }

                    if (item.CompareTo(_items[childItemToSwapIndex]) < 0)
                        Swap(item, _items[childItemToSwapIndex]);
                    else
                        return;
                }
                else
                    return;
            }

            bool isItemExists(int itemIndex)
            {
                return itemIndex < _itemsCount;
            }
        }

        /// <summary>
        /// Replaces the 2 items positions in the tree
        /// </summary>
        void Swap(T itemA, T itemB)
        {
            _items[itemA.HeapIndex] = itemB;
            _items[itemB.HeapIndex] = itemA;
        
            int auxHeapIndex = itemA.HeapIndex;
            itemA.HeapIndex = itemB.HeapIndex;
            itemB.HeapIndex = auxHeapIndex;
        }
    }
}
