// Copyright (c) 2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering
{
    public class QueryManager : IDisposable
    {
        private struct QueryEvent
        {
            public QueryPool Pool;

            public int Index;

            public ProfilingState ProfilingState;
        }

        private const int TimestampQueryPoolCapacity = 64;

        private readonly CommandList commandList;
        private readonly GraphicsResourceAllocator allocator;      
        private readonly long[] queryResults = new long[TimestampQueryPoolCapacity];
        private readonly Queue<QueryEvent> queryEvents = new Queue<QueryEvent>();
        private readonly Stack<QueryEvent> queries = new Stack<QueryEvent>();

        private QueryPool currentQueryPool;
        private int currentQueryIndex;

        public QueryManager(CommandList commandList, GraphicsResourceAllocator allocator)
        {
            this.commandList = commandList;
            this.allocator = allocator;
        }

        /// <summary>
        /// Begins profile.
        /// </summary>
        /// <param name="profileColor">The profile event color.</param>
        /// <param name="profilingKey">The <see cref="ProfilingKey"/></param>
        public Scrope BeginProfile(Color4 profileColor, ProfilingKey profilingKey)
        {
            if (!Profiler.IsEnabled(profilingKey))
            {
                return new Scrope(this, profilingKey);
            }

            // Allocate two timestamp queries
            if (currentQueryPool == null || currentQueryIndex > currentQueryPool.QueryCount - 2)
            {
                currentQueryPool = allocator.GetQueryPool(QueryType.Timestamp, TimestampQueryPoolCapacity);
                currentQueryIndex = 0;
            }

            // Push the current query range onto the stack 
            queries.Push(new QueryEvent
            {
                ProfilingState = Profiler.New(profilingKey),
                Pool = currentQueryPool,
                Index = currentQueryIndex
            });

            // Query the timestamp at the beginning of the range
            commandList.WriteTimestamp(currentQueryPool, currentQueryIndex);

            // Advance next allocation index;
            currentQueryIndex += 2;

            // Sets a debug marker if debug mode is enabled
            if (commandList.GraphicsDevice.IsDebugMode)
            {
                commandList.BeginProfile(profileColor, profilingKey.Name);
            }

            return new Scrope(this, profilingKey);
        }

        /// <summary>
        /// Ends profile.
        /// </summary>
        public void EndProfile(ProfilingKey profilingKey)
        {
            if (!Profiler.IsEnabled(profilingKey))
            {
                return;
            }

            if (queries.Count == 0)
            {
                throw new InvalidOperationException();
            }

            // Get the current query
            var query = queries.Pop();

            // Query the timestamp at the end of the range
            commandList.WriteTimestamp(query.Pool, query.Index + 1);

            // Add the queries to the list of queries to proceess
            queryEvents.Enqueue(query);

            // End the debug marker
            if (commandList.GraphicsDevice.IsDebugMode)
            {
                commandList.EndProfile();
            }
        }

        public void Flush()
        {
            QueryPool pool = null;

            while (queryEvents.Count > 0)
            {
                var query = queryEvents.Peek();

                // If the query is allocated from a new pool, read back it's data
                if (query.Pool != pool)
                {
                    // Don't read back the pool we are currently recording to
                    if (query.Pool == currentQueryPool)
                        return;

                    // If the pool is not ready yet, wait until next time
                    if (!query.Pool.TryGetData(queryResults))
                        return;

                    // Recycle the pool
                    pool = query.Pool;
                    allocator.ReleaseReference(pool);
                }

                // Remove successful queries
                queryEvents.Dequeue();

                // Profile
                query.ProfilingState.Begin((double)queryResults[query.Index] / commandList.GraphicsDevice.TimestampFrequency);
                query.ProfilingState.End((double)queryResults[query.Index + 1] / commandList.GraphicsDevice.TimestampFrequency);
            }
        }

        public void Dispose()
        {
            QueryPool pool = null;

            while (queryEvents.Count > 0)
            {
                var query = queryEvents.Dequeue();
                if (query.Pool != pool)
                {
                    pool = query.Pool;
                    allocator.ReleaseReference(pool);
                }
            }

            if (currentQueryPool != pool)
            {
                allocator.ReleaseReference(currentQueryPool);
            }

            currentQueryPool = null;
        }

        public struct Scrope : IDisposable
        {
            private readonly QueryManager queryManager;
            private readonly ProfilingKey profilingKey;

            public Scrope(QueryManager queryManager, ProfilingKey profilingKey)
            {
                this.queryManager = queryManager;
                this.profilingKey = profilingKey;
            }

            public void Dispose()
            {
                queryManager.EndProfile(profilingKey);
            }
        }
    }
}
