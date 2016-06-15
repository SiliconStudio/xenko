using SiliconStudio.Xenko.Engine;

namespace SpriteFonts
{
    class SpriteFontsApp
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
