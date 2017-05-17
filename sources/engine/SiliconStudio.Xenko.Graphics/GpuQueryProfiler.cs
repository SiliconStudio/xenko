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

        private readonly Stack<QueryEvent> queryEventsStack = new Stack<QueryEvent>();
        private readonly FastList<QueryEvent> queryEvents = new FastList<QueryEvent>();

        private CommandList commandList;
        
        public GpuQueryProfiler(CommandList commandList)
        {
            timestampQueryPool = commandList.QueryPoolManager.GetOrCreatePool(commandList, QueryType.Timestamp, TimestampQueryPoolCapacity);

            this.commandList = commandList;
        }

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

        public void BeginProfile(Color4 profileColor, ProfilingKey profilingKey)
        {
            QueryEvent queryEvent = new QueryEvent()
            {
                BeginQuery = timestampQueryPool.AllocateQuery(),
                EndQuery = timestampQueryPool.AllocateQuery(),
                ProfilingState = Profiler.New(profilingKey),
            };
            
            if (queryEvent.BeginQuery.HasValue)
            {
                commandList.WriteTimestamp(timestampQueryPool, queryEvent.BeginQuery.Value);
            }

            queryEventsStack.Push(queryEvent);

            if (commandList.GraphicsDevice.IsDebugMode)
            {
                commandList.BeginProfile(profileColor, profilingKey.Name);
            }
        }

        public void EndProfile()
        {
            if (queryEventsStack.Count == 0)
            {
                return;
            }

            QueryEvent latestQueryEvent = queryEventsStack.Pop();

            if (latestQueryEvent.EndQuery.HasValue)
            {
                commandList.WriteTimestamp(timestampQueryPool, latestQueryEvent.EndQuery.Value);

                if (commandList.GraphicsDevice.IsDebugMode)
                {
                    commandList.EndProfile();
                }
            }

            queryEvents.Add(latestQueryEvent);
        }
    }
}
