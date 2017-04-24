// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// This class represents a graphics adapter.
    /// </summary>
    public sealed partial class GraphicsAdapter : ComponentBase
    {
        private readonly GraphicsOutput[] outputs;

        /// <summary>
        /// Gets the <see cref="GraphicsOutput"/> attached to this adapter
        /// </summary>
        /// <returns>The <see cref="GraphicsOutput"/> attached to this adapter.</returns>
        public GraphicsOutput[] Outputs
        {
            get
            {
                return outputs;
            }
        }

        /// <summary>
        /// Return the description of this adapter
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Description;
        }

        /// <summary>
        /// The unique id in the form of string of this device
        /// </summary>
        public string AdapterUid { get; internal set; }
    }
}
