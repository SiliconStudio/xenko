// Copyright (c) 2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.Collections.Generic;

namespace SiliconStudio.Xenko.Graphics
{
    public class QueryPoolManager
    {
        private readonly List<QueryPool> poolList = new List<QueryPool>();

        /// <summary>
        /// Gets or creates a <see cref="QueryPool"/>, according to the parameters provided.
        /// </summary>
        /// <param name="commandList">The <see cref="CommandList"/>.</param>
        /// <param name="queryType">The <see cref="QueryType"/> of the pool.</param>
        /// <param name="queryCount">The capacity of the pool.</param>
        /// <returns>A recycled or a new <see cref="QueryPool"/>.</returns>
        public QueryPool GetOrCreatePool(CommandList commandList, QueryType queryType, int queryCount)
        {
            foreach (var pool in poolList)
            {
                if (!pool.IsInUse && pool.QueryType == queryType && pool.Capacity >= queryCount)
                {
                    pool.IsInUse = true;
                    return pool;
                }
            }
            var newPool = QueryPool.New(commandList, queryType, queryCount);
            poolList.Add(newPool);

            return newPool;
        }
    }
}
