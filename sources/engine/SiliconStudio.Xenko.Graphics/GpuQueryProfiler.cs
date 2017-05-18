// Copyright (c) 2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.Collections.Generic;

using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Graphics
{
    public class GpuQueryProfiler
    {
        private const int TimestampQueryPoolCapacity = 64;

        private struct QueryEvent
        {
            public Query? BeginQuery;
            public Query? EndQuery;

            public ProfilingState ProfilingState;
        }

        private readonly QueryPool timestampQueryPool;
        
        private long[] queryResults = new long[TimestampQueryPoolCapacity];

        private readonly FastList<QueryEvent> queryEvents = new FastList<QueryEvent>();
        private readonly Stack<QueryEvent> queryEventStack = new Stack<QueryEvent>();

        private readonly CommandList commandList = null;

        public GpuQueryProfiler(CommandList commandList)
        {
            timestampQueryPool = commandList.QueryPoolManager.GetOrCreatePool(commandList, QueryType.Timestamp, TimestampQueryPoolCapacity);

            this.commandList = commandList;
        }

        /// <summary>
        /// Retrieves timestamp from GPU and sends the results to Profiler.
        /// </summary>
        public void SubmitResults()
        {
            timestampQueryPool.GetData(commandList, ref queryResults);
            
            foreach (QueryEvent queryEvent in queryEvents)
            {
                queryEvent.ProfilingState.Begin(queryResults[queryEvent.BeginQuery.Value.InternalIndex]);
                queryEvent.ProfilingState.End(queryResults[queryEvent.EndQuery.Value.InternalIndex]);
            }

            queryEvents.Clear(true);
        }

        /// <summary>
        /// Begins profile.
        /// </summary>
        /// <param name="profileColor">The profile event color.</param>
        /// <param name="profilingKey">The <see cref="ProfilingKey"/></param>
        public void BeginProfile(Color4 profileColor, ProfilingKey profilingKey)
        {
            if (!Profiler.IsEnabled(profilingKey))
            {
                return;
            }

            var queryEvent = new QueryEvent()
            {
                BeginQuery = timestampQueryPool.AllocateQuery(),
                EndQuery = timestampQueryPool.AllocateQuery(),
                ProfilingState = Profiler.New(profilingKey),
            };
            
            // Query might be null if the pool is full
            if (queryEvent.BeginQuery.HasValue)
            {
                commandList.WriteTimestamp(timestampQueryPool, queryEvent.BeginQuery.Value);
            }

            queryEventStack.Push(queryEvent);

            // Sets a debug marker if debug mode is enabled
            if (commandList.GraphicsDevice.IsDebugMode)
            {
                commandList.BeginDebugEvent(profileColor, profilingKey.Name);
            }
        }

        /// <summary>
        /// Ends profile.
        /// </summary>
        public void EndProfile()
        {
            if (queryEventStack.Count == 0)
            {
                return;
            }

            var latestQueryEvent = queryEventStack.Pop();

            if (latestQueryEvent.EndQuery.HasValue)
            {
                commandList.WriteTimestamp(timestampQueryPool, latestQueryEvent.EndQuery.Value);
            }

            if (commandList.GraphicsDevice.IsDebugMode)
            {
                commandList.EndDebugEvent();
            }

            // Adds the event to the event list
            queryEvents.Add(latestQueryEvent);
        }

        public double GetTimestampFrequency()
        {
            #if SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D11
                return timestampQueryPool.GetGpuFrequency(commandList) / 1000.0;
            #else
                return 1000.0;
            #endif
        }
    }
}
