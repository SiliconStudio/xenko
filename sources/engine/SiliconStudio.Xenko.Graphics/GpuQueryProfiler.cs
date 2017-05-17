// Copyright (c) 2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

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

        public GpuQueryProfiler(CommandList commandList)
        {
            timestampQueryPool = commandList.QueryPoolManager.GetOrCreatePool(commandList, QueryType.Timestamp, TimestampQueryPoolCapacity);
        }

        public void SubmitResults(CommandList commandList)
        {
            timestampQueryPool.GetData(commandList, ref queryResults);

            foreach (QueryEvent queryEvent in queryEvents)
            {
                queryEvent.ProfilingState.Begin(queryResults[queryEvent.BeginQuery.Value.InternalIndex]);
                queryEvent.ProfilingState.End(queryResults[queryEvent.EndQuery.Value.InternalIndex]);
            }

            queryEvents.Clear(true);
        }

        public int BeginProfile(CommandList commandList, ProfilingKey profilingKey)
        {
            QueryEvent queryEvent = new QueryEvent()
            {
                BeginQuery = timestampQueryPool.AllocateQuery(),
                EndQuery = timestampQueryPool.AllocateQuery(),
                ProfilingState = Profiler.New(profilingKey),
            };

            int eventId = queryEvents.Count;

            if (queryEvent.BeginQuery.HasValue)
            {
                commandList.WriteTimestamp(timestampQueryPool, queryEvent.BeginQuery.Value);
            }

            queryEvents.Add(queryEvent);

            if (commandList.GraphicsDevice.IsDebugMode)
            {
                commandList.BeginProfile(Color.Green, profilingKey.Name);
            }

            return eventId;
        }

        public void EndProfiler(CommandList commandList, int eventId)
        {
            if (queryEvents[eventId].EndQuery.HasValue)
            {
                commandList.WriteTimestamp(timestampQueryPool, queryEvents[eventId].EndQuery.Value);

                if (commandList.GraphicsDevice.IsDebugMode)
                {
                    commandList.EndProfile();
                }
            }
        }
    }
}
