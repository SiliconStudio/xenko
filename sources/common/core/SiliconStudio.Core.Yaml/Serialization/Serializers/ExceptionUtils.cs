// Copyright (c) 2017 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Reflection;

namespace SiliconStudio.Core.Yaml.Serialization.Serializers
{
    internal static class ExceptionUtils
    {
        /// <summary>
        /// Unwraps some exception such as <see cref="TargetInvocationException"/>.
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static Exception Unwrap(this Exception exception)
        {
            var targetInvocationException = exception as TargetInvocationException;
            if (targetInvocationException != null)
                return targetInvocationException.InnerException;

            return exception;
        }
    }
}