// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.UI
{
    /// <summary>
    /// Provides a services class for all the User Interface elements in Xenko applications.
    /// </summary>
    public struct UIElementServices
    {
        public IServiceRegistry Services { get; set; }

        public bool Equals(ref UIElementServices other)
        {
            return (Services == other.Services);
        }

    }

}
