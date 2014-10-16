// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Shaders.Convertor
{
    /// <summary>
    /// A map rule for types.
    /// </summary>
    public class MapRule
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public string Type { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format("<map name='{0}' type='{1}'/>", this.Name, this.Type);
        }
    }
}
