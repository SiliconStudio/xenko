// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
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
