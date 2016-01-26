using System.Collections.Generic;

namespace RenderArchitecture
{
    public class RenderStage
    {
        public string Name { get; }
        public EffectPermutationSlot EffectSlot { get; set; }

        /// <summary>
        /// Index in <see cref="RootRenderFeature.RenderStages"/>.
        /// </summary>
        public int Index = -1;

        public RenderStage(string name)
        {
            Name = name;
        }
    }
}