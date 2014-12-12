// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Reflection;

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Effects.Images
{
    /// <summary>
    /// Base class for a <see cref="ColorTransform"/> to be used in a <see cref="ColorTransformGroup"/>.
    /// </summary>
    [DataContract("ColorTransform")]
    public class ColorTransform
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ColorTransform" /> class.
        /// </summary>
        /// <param name="colorTransformShader">Name of the shader.</param>
        /// <exception cref="System.ArgumentNullException">shaderName</exception>
        public ColorTransform(string colorTransformShader)
        {
            if (colorTransformShader == null) throw new ArgumentNullException("colorTransformShader");
            Parameters = new ParameterCollection();

            // Initialize all Parameters with values coming from each ParameterKey
            InitializeProperties();

            Shader = colorTransformShader;
        }

        /// <summary>
        /// Gets or sets the name of the shader.
        /// </summary>
        /// <value>The name of the shader.</value>
        [DataMemberIgnore]
        public string Shader
        {
            get
            {
                return Parameters.Get(ColorTransformKeys.Shader);
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                Parameters.Set(ColorTransformKeys.Shader, value);
            }
        }


        /// <summary>
        /// Gets the parameters.
        /// </summary>
        /// <value>The parameters.</value>
        [DataMemberIgnore]
        public ParameterCollection Parameters { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ColorTransform"/> is enabled.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        public bool Enabled
        {
            get
            {
                return Parameters.Get(ColorTransformKeys.Enabled);
            }
            set
            {
                Parameters.Set(ColorTransformKeys.Enabled, value);
            }
        }

        /// <summary>
        /// Updates the parameters for this transformation.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <remarks>This method is called just before rendering the ColorTransformGroup that is holding this ColorTransform</remarks>
        public virtual void UpdateParameters(ColorTransformContext context)
        {
        }

        private void InitializeProperties()
        {
            foreach (var property in GetType().GetTypeInfo().DeclaredProperties)
            {
                if (property.CanRead && property.CanWrite)
                {
                    property.SetValue(this, property.GetValue(this));
                }
            }
        }
    }
}