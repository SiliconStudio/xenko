// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.IO;

namespace SiliconStudio.Xenko.ProjectGenerator
{
    public partial class ResharperDotSettings
    {
        public Guid FileInjectedGuid { get; set; }

        public FileInfo SharedSolutionDotSettings { get; set; }
    }
}
