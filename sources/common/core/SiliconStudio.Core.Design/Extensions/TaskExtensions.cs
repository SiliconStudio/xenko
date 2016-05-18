// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Threading.Tasks;

namespace SiliconStudio.Core.Extensions
{
    public static class TaskExtensions
    {
        public static void Forget(this Task task)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
        }
    }
}
