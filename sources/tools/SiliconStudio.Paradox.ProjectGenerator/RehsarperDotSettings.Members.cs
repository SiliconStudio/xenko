// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.IO;

namespace SiliconStudio.Paradox.ProjectGenerator
{
    public partial class ResharperDotSettings
    {
        public Guid FileInjectedGuid { get; set; }

        public FileInfo SharedSolutionDotSettings { get; set; }
    }
}