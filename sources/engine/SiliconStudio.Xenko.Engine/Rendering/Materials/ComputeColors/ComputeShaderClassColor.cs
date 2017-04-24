// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Rendering.Materials.ComputeColors
{
    /// <summary>
    /// A shader outputing a single scalar value.
    /// </summary>
    [DataContract("ComputeShaderClassColor")]
    [Display("Shader")]
    public class ComputeShaderClassColor : ComputeShaderClassBase<IComputeColor>, IComputeColor
    {
        private int hashCode = 0;

        /// <inheritdoc/>
        public bool HasChanged
        {
            get
            {
                if (hashCode != 0 && hashCode == (MixinReference?.GetHashCode() ?? 0))
                    return false;

                hashCode = (MixinReference?.GetHashCode() ?? 0);
                return true;
            }
        }
    }
}
