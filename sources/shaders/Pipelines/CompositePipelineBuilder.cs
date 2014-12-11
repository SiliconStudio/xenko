using System.Collections;
using System.Collections.Generic;

namespace SiliconStudio.Paradox.Effects.Pipelines
{
    public class CompositePipelineBuilder : PipelineBuilder, IEnumerable<PipelineBuilder>
    {
        public List<PipelineBuilder> Children { get; private set; }

        public CompositePipelineBuilder()
        {
            Children = new List<PipelineBuilder>();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<PipelineBuilder> GetEnumerator()
        {
            return Children.GetEnumerator();
        }

        public void Add(PipelineBuilder pipelineBuilder)
        {
            Children.Add(pipelineBuilder);
        }

        public override void Load()
        {
            // Load each child pipeline
            foreach (var child in Children)
            {
                child.ServiceRegistry = ServiceRegistry;

                // Only because it's temporary code...
                // We would probably need a context?
                if (child.Pipeline == null)
                    child.Pipeline = Pipeline;

                child.Load();
            }
        }

        public override void Unload()
        {
            // Unload in opposite order
            for (int index = Children.Count - 1; index >= 0; index--)
            {
                var child = Children[index];
                child.Unload();
            }
        }
    }
}