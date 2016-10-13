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
    public class NugetLogger : NuGet.ILogger, IPackageManagerLogger
    {
        private readonly IPackageManagerLogger _logger;
        private readonly NuGet.ILogger _nativeLogger;

        private NugetLogger()
        {
            
        }
        public NugetLogger(IPackageManagerLogger logger)
        {
            _logger = logger;
        }

        public NugetLogger(NuGet.ILogger logger)
        {
            _nativeLogger = logger;
        }

        public void Log(MessageLevel level, string message)
        {
            _nativeLogger?.Log((NuGet.MessageLevel) level, message, null);
        }

        public void Log(NuGet.MessageLevel level, string message, params object[] args)
        {
            _logger?.Log((MessageLevel) level, message);
        }

        public FileConflictResolution ResolveFileConflict(string message)
        {
            return FileConflictResolution.Ignore;
        }

        public static readonly IPackageManagerLogger NullInstance = new NugetLogger();
    }
}
