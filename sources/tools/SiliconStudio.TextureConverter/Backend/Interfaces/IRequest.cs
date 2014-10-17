// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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
