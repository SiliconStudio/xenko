using SiliconStudio.Xenko.Engine;

namespace RenderSceneToTexture
{
    class RenderSceneToTextureApp
    {
        static void Main(string[] args)
        {
            // Profiler.EnableAll();
            using (var game = new Game())
            {
                game.Run();
            }
        }
    }
}
