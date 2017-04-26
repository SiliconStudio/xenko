// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using SiliconStudio.Assets;
using SiliconStudio.Xenko.Input.Mapping;

namespace SiliconStudio.Xenko.Assets.Input
{
    class DefaultInputActionConfigurationFactory : AssetFactory<InputActionConfigurationAsset>
    {
        public override InputActionConfigurationAsset New()
        {
            return new InputActionConfigurationAsset
            {
                Actions = new List<InputAction>()
            };
        }
    }
}
