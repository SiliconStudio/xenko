namespace SiliconStudio.Xenko.Engine.NextGen
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
