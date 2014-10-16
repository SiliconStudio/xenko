// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiliconStudio.Core.Diagnostics
{
    class NullLogger : Logger
    {
        protected override void LogRaw(ILogMessage logMessage)
        {
            // Discard the message
        }
    }
}
