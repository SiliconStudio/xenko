// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Core.Collections
{
    /// <summary>
    /// Represents a node in a priority queue, to allow O(n) removal.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PriorityQueueNode<T>
    {
        public T Value;

        public int Index { get; internal set; }

        public PriorityQueueNode(T value)
        {
            Value = value;
            Index = -1;
        }
    }
}