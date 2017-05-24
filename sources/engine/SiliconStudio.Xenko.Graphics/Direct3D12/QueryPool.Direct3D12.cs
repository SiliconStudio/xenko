// Copyright (c) 2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D12

using System;

namespace SiliconStudio.Xenko.Graphics
{
    public partial class QueryPool
    {
        public bool IsFull { get; private set; }

        internal QueryPool InitializeImpl(CommandList commandList, QueryType queryType, int queryCount)
        {
            if (queryCount == 0) throw new ArgumentOutOfRangeException("QueryPool capacity must be > 0");
            return this;
        }

        /// <summary>
        /// Allocate a query from the <see cref="QueryPool"/>.
        /// </summary>
        /// <returns><see cref="Query"/> from the pool; <see cref="null"/> otherwise.</returns>
        public Query? AllocateQuery()
        {
            return null;
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        /// <param name="commandList">The <see cref="CommandList"/>.</param>
        public void Reset(CommandList commandList)
        {
           
        }

        internal void OnRecreateImpl()
        {
            
        }

        /// <summary>
        /// Gets the result of the queries to an array of data.
        /// </summary>
        /// <typeparam name="T">Expected data type returned by a query.</typeparam>
        /// <param name="commandList">The <see cref="CommandList"/>.</param>
        /// <param name="dataArray">A preallocated array of data.</param>
        public void GetData<T>(CommandList commandList, ref T[] dataArray) where T : struct
        {

        }
    }
}
#endif
