// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuGet;

namespace SiliconStudio.PackageManager
{
    public class NugetLogger : ILogger, INugetLogger
    {
        private readonly INugetLogger _logger;
        private readonly ILogger _nativeLogger;

        private NugetLogger()
        {
            
        }
        public NugetLogger(INugetLogger logger)
        {
            _logger = logger;
        }

        public NugetLogger(ILogger logger)
        {
            _nativeLogger = logger;
        }

        public void Log(NugetMessageLevel level, string message)
        {
            _nativeLogger?.Log((MessageLevel) level, message, null);
        }

        public void Log(MessageLevel level, string message, params object[] args)
        {
            _logger?.Log((NugetMessageLevel) level, message);
        }

        public FileConflictResolution ResolveFileConflict(string message)
        {
            return FileConflictResolution.Ignore;
        }

        public static readonly INugetLogger NullInstance = new NugetLogger();
    }
}
