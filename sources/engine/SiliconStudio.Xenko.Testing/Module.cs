using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Testing
{
    class TestingPlugin : IGamePlugin
    {
        public static TestClient TestClient;

        public void Initialize(Game game)
        {
            TestClient = new TestClient(game.Services);
            TestClient.StartClient(game).Wait();
        }

        public void Destroy(Game game)
        {
        }
    }

    internal class Module
    {
        public static TestClient TestClient;

        [ModuleInitializer]
        public static void Initialize()
        {
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
            if (System.AppDomain.CurrentDomain.FriendlyName.StartsWith("SiliconStudio.Assets.CompilerApp")) return;
#endif
            Game.GamePlugins.Add(new TestingPlugin());
        }
    }
}
