using System.Threading.Tasks;

namespace SiliconStudio.Assets.Templates
{
    /// <summary>
    /// An implementation of <see cref="ITemplateGenerator"/> that will save the session and update the assembly references.
    /// An <see cref="AfterSave"/> protected method is provided to do additional work after saving.
    /// </summary>
    public abstract class SessionTemplateGenerator : TemplateGeneratorBase<SessionTemplateGeneratorParameters>
    {
        public sealed override bool Run(SessionTemplateGeneratorParameters parameters)
        {
            var result = Generate(parameters);
            if (!result)
                return false;

            parameters.Session.Save();

            // Load missing references (we do this after saving)
            // TODO: Better tracking of ProjectReferences (added, removed, etc...)
            parameters.Logger.Verbose("Compiling game assemblies...");
            parameters.Session.UpdateAssemblyReferences(parameters.Logger);
            parameters.Logger.Verbose("Game assemblies compiled...");

            result = AfterSave(parameters).Result;

            // Restore NuGet packages for every project not processed yet (esp. Windows/UWP ones)
            parameters.Logger.Verbose("Restoring NuGet packages...");
            foreach (var package in parameters.Session.Packages)
                package.RestoreNugetPackages(parameters.Logger, false);

            return result;
        }

        /// <summary>
        /// Generates the template. This method is called by <see cref="SessionTemplateGenerator.Run"/>, and the session is saved afterward
        /// if the generation is successful.
        /// </summary>
        /// <param name="parameters">The parameters for the template generator.</param>
        /// <remarks>
        /// This method should work in unattended mode and should not ask user for information anymore.
        /// </remarks>
        /// <returns><c>True</c> if the generation was successful, <c>false</c> otherwise.</returns>
        protected abstract bool Generate(SessionTemplateGeneratorParameters parameters);

        /// <summary>
        /// Does additional work after the session has been saved.
        /// </summary>
        /// <param name="parameters">The parameters for the template generator.</param>
        /// <returns>True if the method succeeded, False otherwise.</returns>
        protected virtual Task<bool> AfterSave(SessionTemplateGeneratorParameters parameters)
        {
            return Task.FromResult(true);
        }
    }
}
