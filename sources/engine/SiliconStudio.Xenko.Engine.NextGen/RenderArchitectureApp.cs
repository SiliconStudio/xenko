using RenderArchitecture;
using SiliconStudio.Xenko.Engine;

namespace RenderArchitecture
{
    class RenderArchitectureApp
    {
        static void Main(string[] args)
        {
            // Profiler.EnableAll();
            using (var game = new CustomGame())
            {
                game.Run();
            }
        }
    }
}
