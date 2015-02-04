using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Core.Extensions;

namespace SiliconStudio.BuildEngine
{
    /// <summary>
    /// This class contains extension methods related to the <see cref="BuildStep"/> class.
    /// </summary>
    public static class BuildStepExtensions
    {
        /// <summary>
        /// Enumerates this build step, and all its inner build steps recursively when they are themselves <see cref="EnumerableBuildStep"/>.
        /// </summary>
        /// <param name="buildStep">The build step to enumerates with its inner build steps</param>
        /// <returns>An <see cref="IEnumerable{BuildStep}"/> object enumerating this build step, and all its inner build steps recursively.</returns>
        public static IEnumerable<BuildStep> EnumerateRecursively(this BuildStep buildStep)
        {
            var enumerableBuildStep = buildStep as EnumerableBuildStep;
            if (enumerableBuildStep == null)
                return buildStep.Yield();

            return enumerableBuildStep.Steps.SelectDeep(x => x is EnumerableBuildStep && ((EnumerableBuildStep)x).Steps != null ? ((EnumerableBuildStep)x).Steps : Enumerable.Empty<BuildStep>());
        }
    }
}