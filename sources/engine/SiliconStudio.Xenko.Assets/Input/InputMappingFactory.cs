using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Assets;

namespace SiliconStudio.Xenko.Assets.Input
{
    class DefaultInputMappingFactory : AssetFactory<InputMappingAsset>
    {
        public override InputMappingAsset New()
        {
            return new InputMappingAsset
            {
            };
        }
    }
}
