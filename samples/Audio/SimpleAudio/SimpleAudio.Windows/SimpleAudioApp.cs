using SiliconStudio.Xenko.Engine;

namespace SimpleAudio
{
    class SimpleAudioApp
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
