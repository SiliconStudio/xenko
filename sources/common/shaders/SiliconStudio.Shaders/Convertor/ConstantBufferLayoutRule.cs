// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Shaders.Convertor
{
    /// <summary>
    /// A single map rule.
    /// </summary>
    public class ConstantBufferLayoutRule
    {
        /// <summary>
        /// Gets or sets from name.
        /// </summary>
        /// <value>
        /// From name.
        /// </value>
        public string Register { get; set; }

        /// <summary>
        /// Gets or sets the binding.
        /// </summary>
        /// <value>
        /// The location.
        /// </value>
        public string Binding { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format("Register: {0}, Binding: {1}", this.Register, this.Binding);
        }
    }
}
