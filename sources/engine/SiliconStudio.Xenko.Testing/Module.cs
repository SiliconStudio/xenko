using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Testing
{
    class TestingPlugin : IGamePlugin
    {
        public static TestClient TestClient;

        public void Initialize(Game game, string gameName)
        {
            TestClient = new TestClient(game.Services);
            var foo = TestClient.StartClient(game, gameName);
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
