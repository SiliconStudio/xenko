// Copyright (c) 2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Collections.Generic;

namespace SiliconStudio.Translation.Extractor
{
    internal class Options
    {
        public bool Backup { get; set; }

        public List<string> InputDirs { get; } = new List<string>();

        public List<string> InputFiles { get; } = new List<string>();

        public string OutputFile { get; set; } = "messages.pot";

        public bool Overwrite { get; set; } = true;

        public bool Recursive { get; set; }

        public bool ShowUsage { get; set; }

        public bool Verbose { get; set; }

    }
}
