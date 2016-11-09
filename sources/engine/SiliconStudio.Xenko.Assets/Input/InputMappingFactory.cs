using System.Collections.Generic;
using SiliconStudio.Assets;
using SiliconStudio.Xenko.Input.Mapping;

namespace SiliconStudio.Xenko.Assets.Input
{
    class DefaultInputMappingFactory : AssetFactory<InputMappingAsset>
    {
        public override InputMappingAsset New()
        {
            return new InputMappingAsset
            {
                Actions = new List<InputAction>()
            };
        }
    }
}
