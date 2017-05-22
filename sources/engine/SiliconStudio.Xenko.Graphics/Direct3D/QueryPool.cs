// Copyright (c) 2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D

using System;
using SharpDX.Direct3D11;

namespace SiliconStudio.Xenko.Graphics
{
    public partial class QueryPool
    {
        private const int DisjointQueryCount = 5;

        private int latestAllocatedIndex;

        internal SharpDX.Direct3D11.Query[] nativeObjects;
        
        internal SharpDX.Direct3D11.Query[] disjointQuery;
        private int currentDisjointQueryIndex = 0;
        private int currentDisjointQueryReadIndex = 0;
        private long[] disjointQueryResults = new long[DisjointQueryCount];

        public SharpDX.Direct3D11.Query[] NativeObjects => nativeObjects;

        public bool IsFull { get; private set; }

        internal QueryPool InitializeImpl(CommandList commandList, QueryType queryType, int queryCount)
        {
            if (queryCount == 0) throw new ArgumentOutOfRangeException("QueryPool capacity must be > 0");

            capacity = queryCount;
            // this.queryType = queryType;
            IsFull = false;
            isInUse = true;
            nativeObjects = new SharpDX.Direct3D11.Query[capacity];

            var queryDescription = new QueryDescription();

            switch (queryType)
            {
                case QueryType.Timestamp:
                    queryDescription.Type = SharpDX.Direct3D11.QueryType.Timestamp;
                    break;

                default:
                    throw new NotImplementedException();
            }

            queries = new Query[capacity];

            for (var i = 0; i < capacity; i++)
            {
                queries[i] = new Query()
                {
                    InternalIndex = i,
                };

                nativeObjects[i] = new SharpDX.Direct3D11.Query(NativeDevice, queryDescription);
            }

            // Create disjoint query (required to convert ticks to milliseconds)
            disjointQuery = new SharpDX.Direct3D11.Query[DisjointQueryCount];

            var disjointQueryDescription = new QueryDescription { Type = SharpDX.Direct3D11.QueryType.TimestampDisjoint };
            for (var i = 0; i < DisjointQueryCount; i++)
            {
                disjointQuery[i] = new SharpDX.Direct3D11.Query(NativeDevice, disjointQueryDescription);
            }

            commandList.NativeDeviceContext.Begin(disjointQuery[currentDisjointQueryIndex]);

            return this;
        }

        /// <summary>
        /// Allocate a query from the <see cref="QueryPool"/>.
        /// </summary>
        /// <returns><see cref="Query"/> from the pool; <see cref="null"/> otherwise.</returns>
        public Query? AllocateQuery()
        {
            if (IsFull)
            {
                return null;
            }

            var allocatedObject = queries[latestAllocatedIndex];

            latestAllocatedIndex++;

            if (latestAllocatedIndex >= capacity)
            {
                IsFull = true;
            }

            return allocatedObject;
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        /// <param name="commandList">The <see cref="CommandList"/>.</param>
        public void Reset(CommandList commandList)
        {
            latestAllocatedIndex = 0;
            IsFull = false;

            // D3D11 disjoint queries update, readback, ...
            commandList.NativeDeviceContext.End(disjointQuery[currentDisjointQueryIndex]);
            commandList.NativeDeviceContext.GetData(disjointQuery[currentDisjointQueryReadIndex], out QueryDataTimestampDisjoint result);

            disjointQueryResults[currentDisjointQueryReadIndex] = result.Frequency;

            if (!result.Disjoint && result.Frequency != 0)
            {
                currentDisjointQueryReadIndex++;
                if (currentDisjointQueryReadIndex >= DisjointQueryCount)
                {
                    currentDisjointQueryReadIndex = 0;
                }
            }

            currentDisjointQueryIndex++;
            if (currentDisjointQueryIndex >= DisjointQueryCount)
            {
                currentDisjointQueryIndex = 0;
            }

            commandList.NativeDeviceContext.Begin(disjointQuery[currentDisjointQueryIndex]);
        }

        internal void OnRecreateImpl()
        {
            var queryDescription = new QueryDescription();

            switch (queryType)
            {
                case QueryType.Timestamp:
                    queryDescription.Type = SharpDX.Direct3D11.QueryType.Timestamp;
                    break;

                default:
                    throw new NotImplementedException();
            }

            queries = new Query[capacity];

            for (var i = 0; i < capacity; i++)
            {
                queries[i] = new Query()
                {
                    InternalIndex = i,
                };

                nativeObjects[i] = new SharpDX.Direct3D11.Query(NativeDevice, queryDescription);
            }

            // Create disjoint query (required to convert ticks to milliseconds)
            disjointQuery = new SharpDX.Direct3D11.Query[DisjointQueryCount];

            var disjointQueryDescription = new QueryDescription{ Type = SharpDX.Direct3D11.QueryType.TimestampDisjoint };
            for (var i = 0; i < DisjointQueryCount; i++)
            {
                disjointQuery[i] = new SharpDX.Direct3D11.Query(NativeDevice, disjointQueryDescription);
            }
        }

        /// <summary>
        /// Gets the result of the queries to an array of data.
        /// </summary>
        /// <typeparam name="T">Expected data type returned by a query.</typeparam>
        /// <param name="commandList">The <see cref="CommandList"/>.</param>
        /// <param name="dataArray">A preallocated array of data.</param>
        public void GetData<T>(CommandList commandList, ref T[] dataArray) where T : struct
        {
            for (var index = 0; index < queries.Length; index++)
            {
                commandList.NativeDeviceContext.GetData(nativeObjects[index], out dataArray[index]);
            }

            Reset(commandList);
        }

        /// <summary>
        /// Gets GPU frequency, using D3D11 disjoint query.
        /// </summary>
        /// <returns></returns>
        internal long GetGpuFrequency(CommandList commandList)
        {
            return disjointQueryResults[currentDisjointQueryReadIndex];
        }
    }
}

#endif
