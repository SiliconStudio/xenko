// Copyright (c) 2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using SharpDX.Direct3D11;

namespace SiliconStudio.Xenko.Graphics
{
    public partial class QueryPool
    {
        private int latestAllocatedIndex;

        internal SharpDX.Direct3D11.Query[] nativeObjects;
        
        public SharpDX.Direct3D11.Query[] NativeObjects => nativeObjects;

        public bool IsFull { get; private set; }

        internal QueryPool InitializeImpl(CommandList commandList, QueryType queryType, int queryCount)
        {
            if (queryCount == 0) throw new ArgumentOutOfRangeException("Query Pool capacity must be > 0");

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

            return this;
        }

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

        internal double GetTickToMsRatio()
        {
            return 1000.0;
        }      
    }
}
