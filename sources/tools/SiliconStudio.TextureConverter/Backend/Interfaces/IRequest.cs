// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiliconStudio.TextureConverter
{
    internal abstract class IRequest
    {
        /// <summary>
        /// THe request type, corresponding to the enum <see cref="RequestType"/>
        /// </summary>
        /// <value>
        /// The type of the request.
        /// </value>
        public abstract RequestType Type { get; }
    }
}
