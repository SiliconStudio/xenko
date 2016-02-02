// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering
{
    public partial class TexturingKeys
    {
        static TexturingKeys()
        {
            DefaultTextures = new ReadOnlyCollection<ParameterKey<Texture>>(new List<ParameterKey<Texture>>()
            {
                Texture0,
                Texture1,
                Texture2,
                Texture3,
                Texture4,
                Texture5,
                Texture6,
                Texture7,
                Texture8,
                Texture9,
            });
            TextureCubes = new ReadOnlyCollection<ParameterKey<Texture>>(new List<ParameterKey<Texture>>()
            {
                TextureCube0,
                TextureCube1,
                TextureCube2,
                TextureCube3,
            });
            Textures3D = new ReadOnlyCollection<ParameterKey<Texture>>(new List<ParameterKey<Texture>>()
            {
                Texture3D0,
                Texture3D1,
                Texture3D2,
                Texture3D3,
            });
        }

        /// <summary>
        /// Default textures used by this class (<see cref="Texture0"/>, <see cref="Texture1"/>...etc.)
        /// </summary>
        public static readonly IReadOnlyList<ParameterKey<Texture>> DefaultTextures;

        /// <summary>
        /// The cube textures used by this class (<see cref="TextureCube0"/>, <see cref="TextureCube1"/>...etc.)
        /// </summary>
        public static readonly IReadOnlyList<ParameterKey<Texture>> TextureCubes;

        /// <summary>
        /// The 3d textures used by this class (<see cref="Texture3D0"/>, <see cref="Texture3D1"/>...etc.)
        /// </summary>
        public static readonly IReadOnlyList<ParameterKey<Texture>> Textures3D;
    }
}