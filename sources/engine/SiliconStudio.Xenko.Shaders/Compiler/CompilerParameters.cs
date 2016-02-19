// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Shaders.Compiler
{
    /// <summary>
    /// Parameters used for compilation.
    /// </summary>
    [DataContract]
    public sealed class CompilerParameters : ParameterCollection
    {
        /// <summary>
        /// The compiler platform type.
        /// </summary>
        public static readonly PermutationParameterKey<GraphicsPlatform> GraphicsPlatformKey = ParameterKeys.NewPermutation<GraphicsPlatform>();

        /// <summary>
        /// The graphics profile target type.
        /// </summary>
        public static readonly PermutationParameterKey<GraphicsProfile> GraphicsProfileKey = ParameterKeys.NewPermutation<GraphicsProfile>();

        /// <summary>
        /// The debug flag.
        /// </summary>
        public static readonly PermutationParameterKey<bool> DebugKey = ParameterKeys.NewPermutation(true);

        /// <summary>
        /// Initializes a new instance of the <see cref="CompilerParameters"/> class.
        /// </summary>
        public CompilerParameters()
        {
            Platform = GraphicsPlatform.Direct3D11;
            Profile = GraphicsProfile.Level_11_0;
        }

        /// <summary>
        /// Gets or sets the priority (in case this compile is scheduled in a custom async pool)
        /// </summary>
        /// <value>
        /// The priority.
        /// </value>
        [DataMemberIgnore]
        public int TaskPriority { get; set; }

        /// <summary>
        /// The shader target type
        /// </summary>
        [DataMemberIgnore]
        public GraphicsPlatform Platform
        {
            get
            {
                return Get(GraphicsPlatformKey);
            }

            set
            {
                Set(GraphicsPlatformKey, value);
            }
        }

        /// <summary>
        /// The shader target type
        /// </summary>
        [DataMemberIgnore]
        public GraphicsProfile Profile
        {
            get
            {
                return Get(GraphicsProfileKey);
            }

            set
            {
                Set(GraphicsProfileKey, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether shader must be compiled with debug info.
        /// </summary>
        /// <value><c>true</c> if shader must be compiled with debug info; otherwise, <c>false</c>.</value>
        [DataMemberIgnore]
        public bool Debug
        {
            get
            {
                return Get(DebugKey);
            }

            set
            {
                Set(DebugKey, value);
            }
        }
    }
}