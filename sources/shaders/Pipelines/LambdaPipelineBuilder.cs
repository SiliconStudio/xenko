using System;

namespace SiliconStudio.Paradox.Effects.Pipelines
{
    public class LambdaPipelineBuilder : PipelineBuilder
    {
        private Action<PipelineBuilder> loadAction;

        public LambdaPipelineBuilder(Action<PipelineBuilder> loadAction)
        {
            this.loadAction = loadAction;
        }

        public override void Load()
        {
            loadAction(this);
        }
    }
}