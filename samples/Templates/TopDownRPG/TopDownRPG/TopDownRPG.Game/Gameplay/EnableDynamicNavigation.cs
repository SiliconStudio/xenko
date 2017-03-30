using System.Linq;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Navigation;

namespace Gameplay
{
    public class EnableDynamicNavigation : StartupScript
    {
        public override void Start()
        {
            Game.GameSystems.OfType<DynamicNavigationMeshSystem>().FirstOrDefault().Enabled = true;
        }
    }
}
