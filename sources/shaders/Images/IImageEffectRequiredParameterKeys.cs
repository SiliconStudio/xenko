using System.Collections.Generic;

namespace SiliconStudio.Paradox.Effects.Images
{
    /// <summary>
    /// An interface to provide <see cref="ParameterKey"/> dependencies.
    /// </summary>
    public interface IImageEffectRequiredParameterKeys
    {
        /// <summary>
        /// Fills the parameter key dependencies if this effect is depending on a ParameterKey set on the
        /// <see cref="ImageEffectContext.Parameters" />. See remarks.
        /// </summary>
        /// <param name="requiredKeys">The required keys.</param>
        /// <remarks>An effect can be composed of several sub-effects/components that may require the computation
        /// result of a previous effect (or a special input texture), that is not provided by the effect itself.
        /// <see cref="ImageEffectContext.Parameters" /> allows to share parameters inside a same context.
        /// This methods allows to describe this implicit dependencies by providing the list of
        /// <see cref="ParameterKey" /> that are required indirectly by this effect.
        /// By default this methods does not fill any <see cref="ParameterKey" /> dependencies.</remarks>
        void FillRequired(HashSet<ParameterKey> requiredKeys);
    }
}