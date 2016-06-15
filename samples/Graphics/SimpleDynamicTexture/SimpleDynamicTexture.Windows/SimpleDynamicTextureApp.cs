using SiliconStudio.Xenko.Engine;

namespace SimpleDynamicTexture
{
    class SimpleDynamicTextureApp
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
