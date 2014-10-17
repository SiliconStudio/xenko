// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.BuildEngine
{
    public class BuildStepEventArgs : EventArgs
    {
        public BuildStepEventArgs(BuildStep step, ILogger logger)
        {
            Step = step;
            Logger = logger;
        }

        public BuildStep Step { get; private set; }

        public ILogger Logger { get; set; }
    }
}