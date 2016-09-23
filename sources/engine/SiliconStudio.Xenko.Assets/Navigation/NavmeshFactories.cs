using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Assets;

namespace SiliconStudio.Xenko.Assets.Navigation
{
    public class DefaultNavmeshFactory : AssetFactory<NavmeshAsset>
    {
        public override NavmeshAsset New()
        {
            return new NavmeshAsset();
        }
    }

}
