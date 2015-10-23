// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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

        public BuildParameterCollection Parameters { get; private set; }

        public int MaxParallelProcesses { get; }

        private int spawnedProcessCount;

        public BuilderContext(string buildPath, string buildProfile, FileVersionTracker inputHashes, BuildParameterCollection parameters, int maxParallelProcess, string slaveBuilderPath)
        {
            BuildPath = buildPath;
            BuildProfile = buildProfile;
            InputHashes = inputHashes;
            Parameters = parameters;
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