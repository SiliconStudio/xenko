using Microsoft.CodeAnalysis;

namespace SiliconStudio.Xenko.Assets.Scripts
{
    public struct SlotGeneratorContext
    {
        public SlotGeneratorContext(Compilation compilation)
        {
            Compilation = compilation;
        }

        public Compilation Compilation { get; }
    }
}