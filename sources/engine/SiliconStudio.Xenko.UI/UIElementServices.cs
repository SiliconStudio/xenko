// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.UI
{
    /// <summary>
    /// Provides a services class for all the User Interface elements in Xenko applications.
    /// </summary>
    internal struct UIElementServices
    {
        public IServiceRegistry Services { get; set; }

        public bool Equals(ref UIElementServices other)
        {
            return (Services == other.Services);
        }

    }

}
