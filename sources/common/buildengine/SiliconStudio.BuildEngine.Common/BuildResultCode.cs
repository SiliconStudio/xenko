// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
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
