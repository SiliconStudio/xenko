// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Presentation.Quantum.View
{
    /// <summary>
    /// An implementation of the <see cref="ObservableNodeTemplateProvider"/> that matches every <see cref="IObservableNode"/>.
    /// </summary>
    public class AnyNodeTemplateProvider : ObservableNodeTemplateProvider
    {
        /// <inheritdoc/>
        public override string Name { get { return "AnyNodeTemplateProvider" + Suffix; } }

        /// <summary>
        /// Gets or sets a suffix for this template provider to be used to create its unique <see cref="Name"/>.
        /// </summary>
        public string Suffix { get; set; }

        /// <inheritdoc/>
        public override bool MatchNode(IObservableNode node)
        {
            return true;
        }
    }
}