using SiliconStudio.Xenko.Engine;

namespace HelloWorld
{
    class HelloWorldApp
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
