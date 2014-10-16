// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;

namespace SiliconStudio
{
    public class ProcessOutputs
    {
        public List<string> OutputLines { get; private set; }

        public List<string> OutputErrors { get; private set; }

        public int ExitCode { get; set; }

        public ProcessOutputs()
        {
            OutputLines = new List<string>();
            OutputErrors = new List<string>();
        }
    }
}