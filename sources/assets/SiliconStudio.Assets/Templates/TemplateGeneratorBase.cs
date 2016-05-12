using System.Threading.Tasks;

namespace SiliconStudio.Assets.Templates
{
    /// <summary>
    /// Base implementation for <see cref="ITemplateGenerator"/> and <see cref="ITemplateGenerator{TParameters}"/>.
    /// </summary>
    /// <typeparam name="TParameters">The type of parameters this generator uses.</typeparam>
    public abstract class TemplateGeneratorBase<TParameters> : ITemplateGenerator<TParameters> where TParameters : TemplateGeneratorParameters
    {
        /// <inheritdoc/>
        public abstract bool IsSupportingTemplate(TemplateDescription templateDescription);

        /// <inheritdoc/>
        public abstract Task<bool> PrepareForRun(TParameters parameters);

        /// <inheritdoc/>
        public abstract bool Run(TParameters parameters);
    }
}