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
        private readonly string parameterKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialStreamAttribute" /> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="type">The type.</param>
        /// <param name="parameterKey">The parameter key.</param>
        public MaterialStreamAttribute(string stream, MaterialStreamType type, string parameterKey = null)
        {
            this.stream = stream;
            this.type = type;
            this.parameterKey = parameterKey;
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

        public string ParameterKey
        {
            get
            {
                return parameterKey;
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