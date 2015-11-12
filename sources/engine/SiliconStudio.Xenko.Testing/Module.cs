using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Testing
{
    internal class Module
    {
        public static TestClient TestClient;

        [ModuleInitializer]
        public static void Initialize()
        {
            if (System.AppDomain.CurrentDomain.FriendlyName.StartsWith("SiliconStudio.Assets.CompilerApp")) return;
            Task.Run(async () =>
            {
                while (true)
                {
                    if (Game.CurrentGame != null) //todo is there a better way to do this??
                    {
                        TestClient = new TestClient(Game.CurrentGame.Services);
                        await TestClient.StartClient(Game.CurrentGame);
                        return;
                    }
                    await Task.Delay(500);
                }
            });
        }
    }
}
