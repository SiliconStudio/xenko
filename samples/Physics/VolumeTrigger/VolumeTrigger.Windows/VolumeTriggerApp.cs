using SiliconStudio.Xenko.Engine;

namespace VolumeTrigger
{
    class VolumeTriggerApp
    {
        static void Main(string[] args)
        {
            using (var game = new Game())
            {
                game.Run();
            }
        }
    }
}
