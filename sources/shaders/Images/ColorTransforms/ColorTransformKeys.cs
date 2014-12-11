// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Paradox.Effects.Images
{
    /// <summary>
    /// Parameter keys used by <see cref="ColorTransform"/>
    /// </summary>
    internal static class ColorTransformKeys
    {
        /// <summary>
        /// A boolean indicating wheter a <see cref="ColorTransform"/> is active or not.
        /// </summary>
        public static readonly ParameterKey<bool> Enabled = ParameterKeys.New(true);

        /// <summary>
        /// The shader used by <see cref="ColorTransform"/>.
        /// </summary>
        public static readonly ParameterKey<string> Shader = ParameterKeys.New("ColorTransformShader");
    }
}