using System;

namespace Client
{
    public class ArrayList<T>
    {
        public T[] Elements { get; private set; } = new T[12];
        public uint Count { get; private set; } = 0;

        public uint Capacity => (uint)Elements.Length;

        public void Add(T elem)
        {
            if (Count >= Elements.Length)
            {
                throw new Exception("ArrayList is full");
            }

            Elements[Count] = elem;
            Count++;
        }

        public void Clear()
        {
            Count = 0;
        }
    }
}