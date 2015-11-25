// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

namespace SiliconStudio.Core
{
    /// <summary>
    /// Extension methods for <see cref="Guid"/>.
    /// </summary>
    public static class GuidExtensions
    {
        private static readonly Dictionary<Guid, int> RuntimeIds = new Dictionary<Guid, int>();

        /// <summary>
        /// Computes an unique runtime id (32bits) valid for the specified <see cref="Guid"/>.
        /// </summary>
        /// <param name="guid">A full unique identifier</param>
        /// <returns>An unique runtime identifier (32bits)</returns>
        public static int ToRuntimeId(this Guid  guid)
        {
            int runtimeId = 0;
            lock (RuntimeIds)
            {
                if (!RuntimeIds.TryGetValue(guid, out runtimeId))
                {
                    runtimeId = RuntimeIds.Count + 1;
                    RuntimeIds.Add(guid, runtimeId);
                }
            }
            return runtimeId;
        }
    }
}