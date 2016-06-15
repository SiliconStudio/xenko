
namespace DeferredLighting
{
    class DeferredLightingApp
    {
        static void Main(string[] args)
        {
            // Profiler.EnableAll();
            using (var game = new DeferredLightingGame())
            {
                game.Run();
            }
        }
    }
}
