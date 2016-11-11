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
