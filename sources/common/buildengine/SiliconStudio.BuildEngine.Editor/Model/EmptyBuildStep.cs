using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SiliconStudio.BuildEngine.Editor.Model
{
    public class EmptyBuildStep : BuildStep, IDisposable
    {
        public override string Title { get { return "<No build step>"; } }

        public static List<WeakReference<EmptyBuildStep>> UsageList = new List<WeakReference<EmptyBuildStep>>();

        public EmptyBuildStep()
        {
            UsageList.Add(new WeakReference<EmptyBuildStep>(this));
        }

        public void ReplaceWith(BuildStep step)
        {
            if (step.Parent != null)
            {
                step.Parent.RemoveChild(step);
            }
            PropertyInfo propertyInfo = Parent.GetBuildStepProperties().Single(x => x.GetValue(Parent) == this);
            propertyInfo.SetValue(Parent, step);
        }

        public override Task<ResultStatus> Execute(IExecuteContext executeContext, BuilderContext builderContext)
        {
            throw new NotSupportedException();
        }

        public override BuildStep Clone()
        {
            throw new NotSupportedException();
        }

        public override void RemoveChild(BuildStep child)
        {
            throw new InvalidOperationException("An EmptyBuildStep can't have a child BuildStep.");
        }

        public override string ToString()
        {
            return "<No build step>";
        }

        public void Dispose()
        {
            foreach (WeakReference<EmptyBuildStep> weakStepRef in UsageList.ToArray())
            {
                EmptyBuildStep step;
                weakStepRef.TryGetTarget(out step);
                if (step == this)
                    UsageList.Remove(weakStepRef);
            }
        }
    }
}
