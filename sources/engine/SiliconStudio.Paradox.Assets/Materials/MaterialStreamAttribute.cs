// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

namespace SiliconStudio.Paradox.Assets.Materials
{
    /// <summary>
    /// An attribute used to identify associate a shader stream with a property
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class MaterialStreamAttribute : Attribute
    {
        private readonly string stream;
        private readonly MaterialStreamType type;

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialStreamAttribute"/> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="type">The type.</param>
        public MaterialStreamAttribute(string stream, MaterialStreamType type)
        {
            this.stream = stream;
            this.type = type;
        }

        /// <summary>
        /// Gets the stream.
        /// </summary>
        /// <value>The stream.</value>
        public string Stream
        {
            get
            {
                return stream;
            }
        }

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>The type.</value>
        public MaterialStreamType Type
        {
            get
            {
                return type;
            }
        }
    }
}