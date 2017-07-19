// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Collections.Generic;
using System.Threading;

using SiliconStudio.Core.Storage;

namespace SiliconStudio.BuildEngine
{
    public class BuilderContext
    {
        internal readonly Dictionary<ObjectId, CommandBuildStep> CommandsInProgress = new Dictionary<ObjectId, CommandBuildStep>();

        internal FileVersionTracker InputHashes { get; private set; }

        public string BuildPath { get; private set; }

        public string BuildProfile { get; private set; }

        public string SlaveBuilderPath { get; private set; }

        public int MaxParallelProcesses { get; }

        private int spawnedProcessCount;

        public BuilderContext(string buildPath, string buildProfile, FileVersionTracker inputHashes, int maxParallelProcess, string slaveBuilderPath)
        {
            BuildPath = buildPath;
            BuildProfile = buildProfile;
            InputHashes = inputHashes;
            SlaveBuilderPath = slaveBuilderPath;
            MaxParallelProcesses = maxParallelProcess;
        }

        public bool CanSpawnParallelProcess()
        {
            if (Interlocked.Increment(ref spawnedProcessCount) > MaxParallelProcesses)
            {
                Interlocked.Decrement(ref spawnedProcessCount);
                return false;
            }
            return true;
        }

        public void NotifyParallelProcessEnded()
        {
            Interlocked.Decrement(ref spawnedProcessCount);
        }
    }
}
