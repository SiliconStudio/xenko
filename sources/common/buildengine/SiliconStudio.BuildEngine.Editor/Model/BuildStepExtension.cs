using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SiliconStudio.BuildEngine.Editor.Model
{
    public static class BuildStepExtension
    {
        /// <summary>
        /// Returns an enumerable of all properties of the given <see cref="BuildStep"/> that can be assigned to a <see cref="BuildStep"/> (except if their type is object), and have both public accessor. The <see cref="BuildStep.Parent"/> property is skipped, and property with indices are also skipped
        /// </summary>
        public static IEnumerable<PropertyInfo> GetBuildStepProperties(this BuildStep buildStep)
        {
            return buildStep.GetType().GetProperties().Where(x =>
                // Has a public setter
                x.SetMethod != null && x.SetMethod.IsPublic &&
                // Has a public getter
                x.GetMethod != null && x.GetMethod.IsPublic &&
                // Is not the "Parent" property
                x.Name != BuildStepPropertiesEnumerator.ParentPropertyName &&
                // Can be assigned a BuildStep
                x.PropertyType.IsAssignableFrom(typeof(BuildStep)) &&
                // But is not of type object
                x.PropertyType != typeof(object) &&
                // Has no index parameter
                x.GetIndexParameters().Length == 0).ToList();
        }


        public static IEnumerable<BuildStep> GetChildSteps(this BuildStep buildStep)
        {
            IEnumerable<BuildStep> result = buildStep.GetBuildStepProperties().Select(x => x.GetValue(buildStep)).Cast<BuildStep>();
            result = buildStep.GetType().GetProperties().Where(
                // Has a public getter
                x => x.GetMethod != null && x.GetMethod.IsPublic &&
                // Is not the "Parent" property
                x.Name != BuildStepPropertiesEnumerator.ParentPropertyName &&
                // Is assignable to an IList<BuildStep>
                typeof(IList<BuildStep>).IsAssignableFrom(x.PropertyType) &&
                // Has no index parameter
                x.GetIndexParameters().Length == 0).Select(
                    // Select the value of the property and aggregate its children to the result
                    x => (IEnumerable<BuildStep>)x.GetValue(buildStep)).Aggregate(result, (current, a) => current.Concat(a));

            // Aggregate the build step itself if it's enumerable
            var enumerable = buildStep as IEnumerable<BuildStep>;
            if (enumerable != null)
                result = result.Concat(enumerable);

            return result;
        }

        public static bool CanAddChildren(this BuildStep step, IEnumerable<BuildStep> children)
        {
            var childIsParent = false;

            foreach (BuildStep item in children)
            {
                // Can't move an item within its own children
                BuildStep parentStep = step;
                while (parentStep != null)
                {
                    if (parentStep == item)
                    {
                        childIsParent = true;
                        break;
                    }

                    parentStep = parentStep.Parent;
                }
            }

            if (childIsParent)
                return false;

            return step is ListBuildStep || step.GetChildSteps().OfType<EmptyBuildStep>().Any();
        }

        public static bool CanInsertChildren(this BuildStep step, IEnumerable<BuildStep> children)
        {
            // Only ListBuildSteps support insersion yet
            return step is ListBuildStep && step.CanAddChildren(children);
        }

        public static void AddChildren(this BuildStep step, IEnumerable<BuildStep> children)
        {
            var parent = step as ListBuildStep;
            var childList = children as BuildStep[] ?? children.ToArray();
            if (parent != null)
            {
                foreach (BuildStep child in childList)
                {
                    if (child.Parent != null)
                        child.Parent.RemoveChild(child);
                    parent.Add(child);
                }
            }

            foreach (BuildStep child in childList)
            {
                var emptyStep = step.GetChildSteps().OfType<EmptyBuildStep>().FirstOrDefault();
                if (emptyStep == null)
                    break;
                emptyStep.ReplaceWith(child);
            }
        }

        public static void InsertChildren(this BuildStep step, IEnumerable<BuildStep> children, int index)
        {
            foreach (BuildStep child in children.Reverse())
            {
                var parentList = child.Parent as ListBuildStep;
                if (parentList != null && parentList == step)
                {
                    int prevIndex = parentList.IndexOf(child);
                    if (prevIndex < index)
                        --index;
                    parentList.Remove(child);
                }
                else if (child.Parent != null)
                {
                    child.Parent.RemoveChild(child);
                }
                ((ListBuildStep)step).Insert(index, child);
            }
        }
    }
}