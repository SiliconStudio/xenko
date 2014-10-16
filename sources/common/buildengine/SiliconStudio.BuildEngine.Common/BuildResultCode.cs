// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiliconStudio.BuildEngine
{
    public enum BuildResultCode
    {
        Successful = 0,
        BuildError = 1,
        CommandLineError = 2,
        Cancelled = 100
    }

}
