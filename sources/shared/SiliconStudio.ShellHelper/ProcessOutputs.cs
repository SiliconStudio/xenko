// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

#if !SILICONSTUDIO_RUNTIME_CORECLR && !SILICONSTUDIO_PLATFORM_UWP
using System;
using System.Collections.Generic;

namespace SiliconStudio
{
    public class ProcessOutputs
    {
        public int ExitCode { get; set; }

        public List<string> OutputLines { get; private set; }

        public List<string> OutputErrors { get; private set; }

        public string OutputAsString { get { return string.Join(Environment.NewLine, OutputLines); } }

        public string ErrorsAsString { get { return string.Join(Environment.NewLine, OutputErrors); } }

        public ProcessOutputs()
        {
            OutputLines = new List<string>();
            OutputErrors = new List<string>();
        }
    }
}
#endif
