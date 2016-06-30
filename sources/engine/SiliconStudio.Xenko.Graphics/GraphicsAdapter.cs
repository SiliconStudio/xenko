// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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
